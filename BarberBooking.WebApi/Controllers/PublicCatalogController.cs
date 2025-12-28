using BarberBooking.Application.Interfaces;
using BarberBooking.Application.UseCases.PublicCatalog.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/public/shops")]
public sealed class PublicCatalogController : ControllerBase
{
    private readonly IPublicCatalogReadService _read;

    public PublicCatalogController(IPublicCatalogReadService read) => _read = read;

    [HttpGet]
    public async Task<IActionResult> GetShops(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _read.GetShopsAsync(new GetPublicShopsQuery(q, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetShopBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var result = await _read.GetShopBySlugAsync(slug, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
