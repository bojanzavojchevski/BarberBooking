namespace BarberBooking.Application.Bookings;

public interface IBookingService
{
    Task<IReadOnlyList<BookingSlotDto>> GetAvailabilityAsync(Guid shopId, Guid barberId,
        Guid serviceId, DateOnly date, CancellationToken ct);
    Task<BookingDto> BookAsync(Guid customerId, CreateBookingRequest request, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> ListMineAsync(Guid customerId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> ListOwnerAsync(Guid ownerId, CancellationToken ct);
    Task CancelAsync(Guid bookingId, Guid userId, bool isOwner, CancellationToken ct);
    Task<IReadOnlyList<WeeklyShiftDto>> GetScheduleAsync(Guid ownerId, Guid barberId, CancellationToken ct);
    Task SetScheduleAsync(Guid ownerId, Guid barberId, ScheduleRequest request, CancellationToken ct);
    Task AddTimeOffAsync(Guid ownerId, Guid barberId, TimeOffRequest request, CancellationToken ct);
}
public sealed class BookingException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public BookingException(int statusCode, string code) : base(code)
    {
        StatusCode = statusCode;
        Code = code;
    }
}
