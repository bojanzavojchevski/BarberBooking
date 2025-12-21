using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/authz-demo")]
public sealed class AuthzDemoController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult AdminOnly() => Ok(new { ok = true, policy = "AdminOnly" });

    [HttpGet("shops/manage")]
    [Authorize(Policy = "CanManageShops")]
    public IActionResult ManageShops() => Ok(new { ok = true, policy = "CanManageShops" });

    [HttpGet("book")]
    [Authorize(Policy = "CanBook")]
    public IActionResult CanBook() => Ok(new { ok = true, policy = "CanBook" });
}
