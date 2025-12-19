using BarberBooking.Application.Auth.Interfaces;
using BarberBooking.Infrastructure.Auth;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace BarberBooking.Infrastructure.Auth;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _db;
    private readonly string _pepper;
    private readonly int _refreshDays;

    public RefreshTokenStore(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _pepper = cfg["RefreshTokens:Pepper"] ?? throw new InvalidOperationException("RefreshTokens:Pepper missing");
        _refreshDays = int.TryParse(cfg["RefreshTokens:Days"], out var d) ? d : 30;
    }

    public async Task<(Guid UserId, DateTime ExpiresAtUtc)?> ValidateAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);

        var entity = await _db.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (entity is null) return null;
        if (entity.RevokedAtUtc is not null) return null;
        if (entity.ExpiresAtUtc <= DateTime.UtcNow) return null;

        return (entity.UserId, entity.ExpiresAtUtc);
    }

    public async Task<(string Token, DateTime ExpiresAtUtc)> IssueAsync(Guid userId, string? ip, string? userAgent, CancellationToken ct)
    {
        var token = GenerateRawToken();
        var now = DateTime.UtcNow;
        var exp = now.AddDays(_refreshDays);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(token),
            CreatedAtUtc = now,
            ExpiresAtUtc = exp,
            CreatedByIp = ip,
            UserAgent = userAgent
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        return (token, exp);
    }

    public async Task RotateAsync(string refreshToken, string newRefreshToken, string? ip, CancellationToken ct)
    {
        var oldHash = Hash(refreshToken);
        var now = DateTime.UtcNow;

        var old = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == oldHash, ct);
        if (old is null) throw new UnauthorizedAccessException("Invalid refresh token.");

        // REUSE detection: ако веќе е revoked -> компромитирано
        if (old.RevokedAtUtc is not null)
        {
            await RevokeAllAsync(old.UserId, "Refresh token reuse detected", ip, ct);
            throw new UnauthorizedAccessException("Refresh token reuse detected.");
        }

        if (old.ExpiresAtUtc <= now)
            throw new UnauthorizedAccessException("Refresh token expired.");

        // најди го новиот токен што штотуку го издадовме
        var newHash = Hash(newRefreshToken);
        var newer = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == newHash, ct);
        if (newer is null) throw new InvalidOperationException("New refresh token not found (issue+rotate mismatch).");

        old.RevokedAtUtc = now;
        old.RevokedByIp = ip;
        old.ReplacedByTokenId = newer.Id;

        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllAsync(Guid userId, string reason, string? ip, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var active = await _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > now)
            .ToListAsync(ct);

        foreach (var t in active)
        {
            t.RevokedAtUtc = now;
            t.RevokedByIp = ip;
        }

        await _db.SaveChangesAsync(ct);
        // reason можеш да го логираш со Serilog (не го чуваме во DB за сега)
    }

    private string Hash(string raw)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw + _pepper);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }

    private static string GenerateRawToken()
    {
        // 32 bytes random -> base64url-like
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
