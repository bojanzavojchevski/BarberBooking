using BarberBooking.Application.UseCases.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/owner/services")]
[Authorize(Policy = "OwnerOnly")]
public sealed class OwnerServicesController : ControllerBase
{
    private readonly CreateServiceUseCase _create;

    public OwnerServicesController(CreateServiceUseCase create) => _create = create;

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _create.ExecuteAsync(request, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message == "owner_has_no_shop")
        {
            return BadRequest(new { error = "owner_has_no_shop" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "service_name_taken")
        {
            return Conflict(new { error = "service_name_taken" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "invalid_service_name")
        {
            return BadRequest(new { error = "invalid_service_name" });
        }
    }
}
