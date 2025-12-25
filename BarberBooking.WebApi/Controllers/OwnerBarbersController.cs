using BarberBooking.Application.UseCases.Barbers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/owner/barbers")]
[Authorize(Policy = "OwnerOnly")]
public sealed class OwnerBarbersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BarberDto>> Create(
        [FromBody] CreateBarberRequest request,
        [FromServices] CreateBarberUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            return Ok(await useCase.ExecuteAsync(request, ct));
        }
        catch (InvalidOperationException ex) when (ex.Message == "owner_has_no_shop")
        {
            return NotFound(new { error = "owner_has_no_shop" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "barber_name_taken")
        {
            return Conflict(new { error = "barber_name_taken" });
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new { error = "invalid_barber" });
        }
        catch (ArgumentException)
        {
            return BadRequest(new { error = "invalid_barber" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BarberSummaryDto>>> List(
        [FromServices] ListMyBarbersUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            return Ok(await useCase.ExecuteAsync(ct));
        }
        catch (InvalidOperationException ex) when (ex.Message == "owner_has_no_shop")
        {
            return NotFound(new { error = "owner_has_no_shop" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BarberDto>> Update(
        Guid id,
        [FromBody] UpdateBarberRequest request,
        [FromServices] UpdateBarberUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            return Ok(await useCase.ExecuteAsync(id, request, ct));
        }
        catch (InvalidOperationException ex) when (ex.Message == "owner_has_no_shop")
        {
            return NotFound(new { error = "owner_has_no_shop" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "barber_not_found")
        {
            return NotFound(new { error = "barber_not_found" });
        }
        catch (InvalidOperationException ex) when (ex.Message == "barber_name_taken")
        {
            return Conflict(new { error = "barber_name_taken" });
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new { error = "invalid_barber" });
        }
        catch (ArgumentException)
        {
            return BadRequest(new { error = "invalid_barber" });
        }
    }
}
