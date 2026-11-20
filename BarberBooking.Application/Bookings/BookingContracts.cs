namespace BarberBooking.Application.Bookings;

public sealed record BookingSlotDto(DateTimeOffset StartUtc, DateTimeOffset EndUtc);
public sealed record CreateBookingRequest(Guid ShopId, Guid BarberId, Guid ServiceId, DateTimeOffset StartUtc);
public sealed record BookingDto(Guid Id, Guid ShopId, Guid BarberId, Guid ServiceId,
    Guid CustomerId, DateTimeOffset StartUtc, DateTimeOffset EndUtc,
    decimal PriceAmount, string Status);
public sealed record WeeklyShiftDto(int DayOfWeek, string Start, string End);
public sealed record ScheduleRequest(IReadOnlyList<WeeklyShiftDto> Shifts);
public sealed record TimeOffRequest(DateTimeOffset StartUtc, DateTimeOffset EndUtc, string? Note);
