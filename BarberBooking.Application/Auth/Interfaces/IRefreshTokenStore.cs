using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.Interfaces;

public interface IRefreshTokenStore
{
    Task<(Guid UserId, DateTime ExpiresAtUtc)?> ValidateAsync(string refreshToken, CancellationToken ct);
    Task<(string Token, DateTime ExpiresAtUtc)> IssueAsync(Guid userId, string? ip, string? userAgent, CancellationToken ct);
    Task RotateAsync(string refreshToken, string newRefreshToken, string? ip, CancellationToken ct);
    Task RevokeAllAsync(Guid userId, string reason, string? ip, CancellationToken ct);
    Task<(Guid UserId, string Token, DateTime ExpiresAtUtc)> RotateAtomicAsync(string refreshToken, string? ip, string? userAgent, CancellationToken ct);
}
