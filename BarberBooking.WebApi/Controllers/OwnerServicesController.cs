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
            return NotFound(new { error = "owner_has_no_shop" });
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


    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceSummaryDto>>> List(
        [FromServices] ListMyServicesUseCase useCase,
        CancellationToken ct)
        => Ok(await useCase.ExecuteAsync(ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> Update(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        [FromServices] UpdateServiceUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            return Ok(await useCase.ExecuteAsync(id, request, ct));
        }
        catch (InvalidOperationException ex) when (ex.Message is "owner_has_no_shop")
        {
            return NotFound(new { error = "owner_has_no_shop" });
        }
        catch (InvalidOperationException ex) when (ex.Message is "service_not_found")
        {
            return NotFound(new { error = "service_not_found" });
        }
        catch (InvalidOperationException ex) when (ex.Message is "service_name_taken")
        {
            return Conflict(new { error = "service_name_taken" });
        }
    }

    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(
    Guid id,
    [FromBody] SetServiceActiveRequest request,
    [FromServices] SetServiceActiveUseCase useCase,
    CancellationToken ct)
    {
        try
        {
            await useCase.ExecuteAsync(id, request.IsActive, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message is "owner_has_no_shop" or "service_not_found")
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteServiceUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            await useCase.ExecuteAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message is "owner_has_no_shop" or "service_not_found")
        {
            return NotFound(new { error = ex.Message });
        }
    }





}
