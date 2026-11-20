using BarberBooking.Application.Bookings;
using BarberBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberBooking.WebApi.Controllers;

[ApiController]
[Route("api/public/availability")]
public sealed class AvailabilityController(IBookingService booking) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingSlotDto>>> Get(
        [FromQuery] Guid shopId, [FromQuery] Guid barberId, [FromQuery] Guid serviceId,
        [FromQuery] DateOnly date, CancellationToken ct)
        => Ok(await booking.GetAvailabilityAsync(shopId, barberId, serviceId, date, ct));
}

[ApiController]
[Route("api/bookings")]
[Authorize(Policy = "CustomerOnly")]
public sealed class BookingsController(IBookingService booking, ICurrentUser current) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(
        [FromBody] CreateBookingRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created,
            await booking.BookAsync(current.UserId, request, ct));

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> Mine(CancellationToken ct)
        => Ok(await booking.ListMineAsync(current.UserId, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await booking.CancelAsync(id, current.UserId, false, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/owner")]
[Authorize(Policy = "OwnerOnly")]
public sealed class OwnerBookingsController(IBookingService booking, ICurrentUser current) : ControllerBase
{
    [HttpGet("bookings")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> List(CancellationToken ct)
        => Ok(await booking.ListOwnerAsync(current.UserId, ct));

    [HttpDelete("bookings/{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await booking.CancelAsync(id, current.UserId, true, ct);
        return NoContent();
    }

    [HttpGet("barbers/{barberId:guid}/schedule")]
    public async Task<ActionResult<IReadOnlyList<WeeklyShiftDto>>> GetSchedule(
        Guid barberId, CancellationToken ct)
        => Ok(await booking.GetScheduleAsync(current.UserId, barberId, ct));

    [HttpPut("barbers/{barberId:guid}/schedule")]
    public async Task<IActionResult> SaveSchedule(
        Guid barberId, [FromBody] ScheduleRequest request, CancellationToken ct)
    {
        await booking.SetScheduleAsync(current.UserId, barberId, request, ct);
        return NoContent();
    }

    [HttpPost("barbers/{barberId:guid}/time-off")]
    public async Task<IActionResult> AddTimeOff(
        Guid barberId, [FromBody] TimeOffRequest request, CancellationToken ct)
    {
        await booking.AddTimeOffAsync(current.UserId, barberId, request, ct);
        return NoContent();
    }
}
