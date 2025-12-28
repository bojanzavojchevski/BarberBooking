using System.Net;
using System.Net.Http.Json;
using BarberBooking.Application.Auth.DTOs;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarberBooking.IntegrationTests;

public sealed class AuthTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;
    private readonly HttpClient _client;

    public AuthTests(TestAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private Task EnsureTestUserAsync()
        => AuthSeeder.EnsureUserAsync(_factory.Services, "test@bb.com", "StrongPass123!", emailConfirmed: true);

    [Fact]
    public async Task Refresh_Rotates_And_Revokes_Old()
    {
        await EnsureTestUserAsync();
        await ResetRefreshTokensAsync();

        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto("test@bb.com", "StrongPass123!"));

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var tokens1 = await login.Content.ReadFromJsonAsync<AuthTokensDto>();
        Assert.NotNull(tokens1);

        var refresh1 = tokens1!.RefreshToken;

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequestDto(refresh1));

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var tokens2 = await refresh.Content.ReadFromJsonAsync<AuthTokensDto>();
        Assert.NotNull(tokens2);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.SingleAsync(u => u.Email == "test@bb.com");

        var lastTwo = await db.RefreshTokens
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(2)
            .ToListAsync();

        Assert.Equal(2, lastTwo.Count);

        var newest = lastTwo[0];
        var previous = lastTwo[1];

        Assert.Equal(previous.FamilyId, newest.FamilyId);

        Assert.NotNull(previous.RevokedAtUtc);
        Assert.NotNull(previous.ReplacedByTokenId);
        Assert.Equal(newest.Id, previous.ReplacedByTokenId);

        Assert.Null(newest.RevokedAtUtc);

        var activeInFamily = await db.RefreshTokens.CountAsync(x =>
            x.UserId == user.Id &&
            x.FamilyId == newest.FamilyId &&
            x.RevokedAtUtc == null &&
            x.ExpiresAtUtc > DateTime.UtcNow);

        Assert.Equal(1, activeInFamily);
    }

    [Fact]
    public async Task Refresh_Reuse_Attack_Revokes_Family_And_Returns_401()
    {
        await EnsureTestUserAsync();
        await ResetRefreshTokensAsync();

        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto("test@bb.com", "StrongPass123!"));
        var t1 = await login.Content.ReadFromJsonAsync<AuthTokensDto>();
        var r1 = t1!.RefreshToken;

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(r1));
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);

        // Reuse old token r1
        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(r1));
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);

        // DB proof: family has no active tokens
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var familyId = (await db.RefreshTokens
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstAsync()).FamilyId;

        var active = await db.RefreshTokens.CountAsync(x => x.FamilyId == familyId && x.RevokedAtUtc == null);
        Assert.Equal(0, active);
    }

    [Fact]
    public async Task Refresh_Expired_Returns_401_Generic()
    {
        await EnsureTestUserAsync();
        await ResetRefreshTokensAsync();

        // Create an expired token row directly
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == "test@bb.com");

            db.RefreshTokens.Add(new BarberBooking.Infrastructure.Auth.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FamilyId = Guid.NewGuid(),
                TokenHash = "DEADBEEF",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
            });

            await db.SaveChangesAsync();
        }

        var res = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto("some-invalid"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    private async Task ResetRefreshTokensAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE "RefreshTokens" RESTART IDENTITY CASCADE;
        """);
    }
}
