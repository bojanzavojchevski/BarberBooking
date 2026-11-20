using BarberBooking.Domain.Exceptions;

namespace BarberBooking.Domain.Entities;

public enum AppointmentStatus { Confirmed = 0, Cancelled = 1 }

public sealed class Appointment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShopId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public decimal PriceAmount { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Confirmed;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    private Appointment() { }

    public static Appointment Create(Guid shopId, Guid barberId, Guid serviceId,
        Guid customerId, DateTimeOffset startUtc, int durationMinutes,
        decimal priceAmount, DateTimeOffset nowUtc)
    {
        if (shopId == Guid.Empty || barberId == Guid.Empty || serviceId == Guid.Empty || customerId == Guid.Empty)
            throw new DomainException("All appointment references are required.");
        if (startUtc.Offset != TimeSpan.Zero || nowUtc.Offset != TimeSpan.Zero)
            throw new DomainException("Booking timestamps must be UTC.");
        if (startUtc <= nowUtc)
            throw new DomainException("Cannot book in the past.");
        if (durationMinutes < 5 || durationMinutes > 480 || durationMinutes % 5 != 0)
            throw new DomainException("Invalid service duration.");
        if (priceAmount < 0)
            throw new DomainException("Invalid price.");

        return new Appointment
        {
            Id = Guid.NewGuid(), ShopId = shopId, BarberId = barberId,
            ServiceId = serviceId, CustomerId = customerId,
            StartUtc = startUtc, EndUtc = startUtc.AddMinutes(durationMinutes),
            PriceAmount = priceAmount, CreatedAtUtc = nowUtc,
            Status = AppointmentStatus.Confirmed
        };
    }

    public void Cancel(DateTimeOffset nowUtc)
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new DomainException("Appointment already cancelled.");
        if (nowUtc.Offset != TimeSpan.Zero)
            throw new DomainException("Cancellation timestamp must be UTC.");
        Status = AppointmentStatus.Cancelled;
        CancelledAtUtc = nowUtc;
    }
}
