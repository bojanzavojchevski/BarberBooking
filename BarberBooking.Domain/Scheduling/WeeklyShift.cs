using BarberBooking.Domain.Exceptions;

namespace BarberBooking.Domain.Scheduling;

public sealed class WeeklyShift
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BarberId { get; private set; }
    // Sunday = 0, Monday = 1, ... Saturday = 6.
    public int DayOfWeek { get; private set; }
    public int StartMinute { get; private set; }
    public int EndMinute { get; private set; }

    private WeeklyShift() { }

    public WeeklyShift(Guid barberId, int dayOfWeek, int startMinute, int endMinute)
    {
        if (barberId == Guid.Empty || dayOfWeek is < 0 or > 6 ||
            startMinute < 0 || endMinute > 1440 || endMinute <= startMinute)
            throw new DomainException("Invalid weekly shift.");
        if (startMinute % 15 != 0 || endMinute % 15 != 0)
            throw new DomainException("Shifts must align to 15-minute intervals.");
        BarberId = barberId;
        DayOfWeek = dayOfWeek;
        StartMinute = startMinute;
        EndMinute = endMinute;
    }
}
