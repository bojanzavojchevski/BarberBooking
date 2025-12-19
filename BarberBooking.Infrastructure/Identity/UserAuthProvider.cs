using BarberBooking.Application.Auth.Interfaces;
using BarberBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Infrastructure.Identity;

public sealed class UserAuthProvider : IUserAuthProvider
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAuthProvider(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<(Guid UserId, string Email)?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || user.Email is null) return null;
        return (user.Id, user.Email);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return false;
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return Array.Empty<string>();
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToArray();
    }

    public async Task<string?> GetEmailByIdAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userManager.Users
            .Select(u => new { u.Id, u.Email })
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user?.Email;
    }
}
