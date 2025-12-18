using BarberBooking.Application.Auth.DTOs;
using BarberBooking.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<ActionResult<AuthTokensDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var client = new ClientContextDto(
            Ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        return Ok(await _auth.LoginAsync(dto, client, ct));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokensDto>> Refresh([FromBody] RefreshRequestDto dto, CancellationToken ct)
    {
        var client = new ClientContextDto(
            Ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        return Ok(await _auth.RefreshAsync(dto, client, ct));
    }
}
