using BarberBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BarberBooking.WebApi.Controllers;

public sealed record RegisterCustomerRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(12)] string Password);

[ApiController]
[Route("api/auth")]
public sealed class RegistrationController(UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth-login-ip")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.FindByEmailAsync(email) is not null)
            return Conflict(new { error = "email_already_registered" });

        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = email, UserName = email };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new
            {
                error = "registration_failed",
                details = result.Errors.Select(x => x.Description).ToArray()
            });

        var roleResult = await users.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
            throw new InvalidOperationException("Failed to assign customer role.");

        // Email confirmation may be requested through the existing endpoint.
        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Registered. You can now sign in.",
            userId = user.Id
        });
    }
}
