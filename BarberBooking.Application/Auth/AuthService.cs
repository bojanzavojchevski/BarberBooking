using BarberBooking.Application.Auth.DTOs;
using BarberBooking.Application.Auth.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserAuthProvider _users;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IRefreshTokenStore _refresh;

    public AuthService(IUserAuthProvider users, IJwtTokenGenerator jwt, IRefreshTokenStore refresh)
    {
        _users = users;
        _jwt = jwt;
        _refresh = refresh;
    }

    public async Task<AuthTokensDto> LoginAsync(LoginRequestDto request, ClientContextDto client, CancellationToken ct)
    {
        var found = await _users.FindByEmailAsync(request.Email, ct);
        if (found is null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var ok = await _users.CheckPasswordAsync(found.Value.UserId, request.Password, ct);
        if (!ok)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await _users.GetRolesAsync(found.Value.UserId, ct);
        var (access, accessExp) = _jwt.CreateAccessToken(found.Value.UserId, found.Value.Email, roles);

        var (refresh, refreshExp) = await _refresh.IssueAsync(found.Value.UserId, client.Ip, client.UserAgent, ct);

        return new AuthTokensDto(access, accessExp, refresh, refreshExp);
    }

    public async Task<AuthTokensDto> RefreshAsync(RefreshRequestDto request, ClientContextDto client, CancellationToken ct)
    {
        // Atomically: validate + reuse-detect + revoke old + issue new (same family)
        var (userId, newRefresh, newRefreshExp) = await _refresh.RotateAtomicAsync(
            request.RefreshToken,
            client.Ip,
            client.UserAgent,
            ct);

        var (email, roles) = await GetEmailAndRoles(userId, ct);
        var (access, accessExp) = _jwt.CreateAccessToken(userId, email, roles);

        return new AuthTokensDto(access, accessExp, newRefresh, newRefreshExp);
    }

    private async Task<(string Email, IReadOnlyCollection<string> Roles)> GetEmailAndRoles(Guid userId, CancellationToken ct)
    {
        var email = await _users.GetEmailByIdAsync(userId, ct);
        if (email is null)
            throw new UnauthorizedAccessException("User not found.");

        var roles = await _users.GetRolesAsync(userId, ct);
        return (email, roles);
    }
}
