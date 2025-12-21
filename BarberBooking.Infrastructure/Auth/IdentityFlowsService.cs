using System.Web;
using BarberBooking.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using BarberBooking.Infrastructure.Identity;

namespace BarberBooking.Infrastructure.Auth;

public sealed class IdentityFlowsService : IIdentityFlowsService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _email;
    private readonly string _baseUrl;

    public IdentityFlowsService(
        UserManager<ApplicationUser> userManager,
        IEmailSender email,
        IOptions<PublicUrlOptions> publicUrl)
    {
        _userManager = userManager;
        _email = email;
        _baseUrl = publicUrl.Value.BaseUrl.TrimEnd('/');
    }

    public async Task RequestEmailConfirmationAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null) return;
        if (user.EmailConfirmed) return;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encoded = HttpUtility.UrlEncode(token);

        var link = $"{_baseUrl}/api/auth/email/confirm?userId={user.Id}&token={encoded}";
        await _email.SendAsync(user.Email!, "Confirm your email", link, ct);
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var decoded = HttpUtility.UrlDecode(token) ?? "";
        var result = await _userManager.ConfirmEmailAsync(user, decoded);
        return result.Succeeded;
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // Non-enumerable
        if (user is null) return;

        // only allow reset if email confirmed
        if (!user.EmailConfirmed) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encoded = HttpUtility.UrlEncode(token);

        var link = $"{_baseUrl}/api/auth/password/reset?email={HttpUtility.UrlEncode(email)}&token={encoded}";
        await _email.SendAsync(user.Email!, "Reset your password", link, ct);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return false;

        var decoded = HttpUtility.UrlDecode(token) ?? "";
        var result = await _userManager.ResetPasswordAsync(user, decoded, newPassword);
        return result.Succeeded;
    }
}
