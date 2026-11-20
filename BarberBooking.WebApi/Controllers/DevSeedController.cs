using BarberBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/dev/seed")]
public sealed class DevSeedController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<Guid>> _roles;
    private readonly IConfiguration _configuration;

    public DevSeedController(
        IWebHostEnvironment env,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<Guid>> roles, IConfiguration configuration)
    {
        _env = env;
        _users = users;
        _roles = roles;
        _configuration = configuration;
    }

    [HttpPost("owner")]
    public async Task<IActionResult> SeedOwner([FromBody] SeedOwnerRequest req,
        [FromHeader(Name = "X-Dev-Seed-Key")] string? providedKey)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        // Never allow unauthenticated owner provisioning with only an email/password.
        var configuredKey = _configuration["DevSeed:Key"];
        if (string.IsNullOrWhiteSpace(configuredKey) ||
            string.IsNullOrWhiteSpace(providedKey))
            return NotFound();

        var expected = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
        var actual = SHA256.HashData(Encoding.UTF8.GetBytes(providedKey));
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            return NotFound();

        var email = req.Email.Trim().ToLowerInvariant();

        // Ensure role exists (just in case)
        if (!await _roles.RoleExistsAsync("Owner"))
            await _roles.CreateAsync(new IdentityRole<Guid>("Owner"));

        var user = await _users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var create = await _users.CreateAsync(user, req.Password);
            if (!create.Succeeded)
                return BadRequest(create.Errors);
        }

        if (!await _users.IsInRoleAsync(user, "Owner"))
            await _users.AddToRoleAsync(user, "Owner");

        return Ok(new { userId = user.Id, email, role = "Owner" });
    }
}

public sealed record SeedOwnerRequest(string Email, string Password);
