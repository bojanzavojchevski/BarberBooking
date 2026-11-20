using BarberBooking.Domain.Exceptions;

namespace BarberBooking.Domain.Scheduling;

public sealed class BarberTimeOff
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BarberId { get; private set; }
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public string? Note { get; private set; }

    private BarberTimeOff() { }

    public BarberTimeOff(Guid barberId, DateTimeOffset startUtc, DateTimeOffset endUtc, string? note)
    {
        if (barberId == Guid.Empty || startUtc.Offset != TimeSpan.Zero ||
            endUtc.Offset != TimeSpan.Zero || endUtc <= startUtc)
            throw new DomainException("Invalid time-off interval.");
        if (note?.Length > 250)
            throw new DomainException("Time-off note is too long.");
        BarberId = barberId;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Note = note?.Trim();
    }
}
