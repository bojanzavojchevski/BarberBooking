using BarberBooking.Application.Auth.DTOs;
using BarberBooking.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IIdentityFlowsService _identityFlows;

    public AuthController(IAuthService auth, IIdentityFlowsService identityFlows)
    {
        _auth = auth;
        _identityFlows = identityFlows;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth-login-ip")]
    public async Task<ActionResult<AuthTokensDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var client = new ClientContextDto(
            Ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        return Ok(await _auth.LoginAsync(dto, client, ct));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth-refresh-ip")]
    public async Task<ActionResult<AuthTokensDto>> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        var client = new ClientContextDto(
            Ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        return Ok(await _auth.RefreshAsync(dto, client, ct));
    }

    [HttpPost("email/confirmation/request")]
    public async Task<IActionResult> RequestEmailConfirmation([FromBody] EmailConfirmationRequestDto dto, CancellationToken ct)
    {
        await _identityFlows.RequestEmailConfirmationAsync(dto.Email, ct);
        return Ok(new { message = "If an account exists, a confirmation link was generated." });
    }

    [HttpPost("email/confirm")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto, CancellationToken ct)
    {
        var ok = await _identityFlows.ConfirmEmailAsync(dto.UserId, dto.Token, ct);
        return ok ? Ok(new { message = "Email confirmed." })
                  : BadRequest(new { message = "Invalid token." });
    }

    [HttpPost("password/forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        await _identityFlows.RequestPasswordResetAsync(dto.Email, ct);
        return Ok(new { message = "If an account exists, a reset link was generated." });
    }

    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 12)
            return BadRequest(new { message = "Invalid token." }); // keep generic outwardly

        var ok = await _identityFlows.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword, ct);
        return ok ? Ok(new { message = "Password reset successful." })
                  : BadRequest(new { message = "Invalid token." });
    }

}
