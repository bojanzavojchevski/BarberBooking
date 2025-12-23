using BarberBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/dev/seed")]
public sealed class DevSeedController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<Guid>> _roles;

    public DevSeedController(
        IWebHostEnvironment env,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<Guid>> roles)
    {
        _env = env;
        _users = users;
        _roles = roles;
    }

    [HttpPost("owner")]
    public async Task<IActionResult> SeedOwner([FromBody] SeedOwnerRequest req)
    {
        if (!_env.IsDevelopment())
            return NotFound(); // don't expose in prod

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
