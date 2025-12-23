using BarberBooking.Application.UseCases.Shops;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/owner/shop")]
[Authorize(Policy = "OwnerOnly")]
public class OwnerShopController : ControllerBase
{
    private readonly CreateMyShopUseCase _create;

    public OwnerShopController(CreateMyShopUseCase create) => _create = create;


    [HttpPost]
    public async Task<ActionResult<ShopDto>> Create(CreateMyShopRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _create.ExecuteAsync(request, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message == "owner_already_has_shop")
        {
            return Conflict(new { error = "owner_already_has_shop" });
        }
    }
}
