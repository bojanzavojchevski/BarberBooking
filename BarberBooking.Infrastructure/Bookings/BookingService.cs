using BarberBooking.Application.Bookings;
using BarberBooking.Domain.Entities;
using BarberBooking.Domain.Scheduling;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Globalization;

namespace BarberBooking.Infrastructure.Bookings;

public sealed class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly TimeZoneInfo _zone;

    public BookingService(AppDbContext db, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _db = db;
        var zoneId = configuration["Booking:TimeZone"] ?? "Europe/Skopje";
        _zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
    }

    private DateOnly TodayLocal =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _zone).Date);

    private static BookingDto ToDto(Appointment b) =>
        new(b.Id, b.ShopId, b.BarberId, b.ServiceId, b.CustomerId,
            b.StartUtc, b.EndUtc, b.PriceAmount, b.Status.ToString());

    private async Task AssertOwnerBarberAsync(Guid ownerId, Guid barberId, CancellationToken ct)
    {
        if (ownerId == Guid.Empty || !await _db.Barbers.AnyAsync(
                b => b.Id == barberId && _db.Shops.Any(
                    s => s.Id == b.ShopId && s.OwnerUserId == ownerId), ct))
            throw new BookingException(404, "barber_not_found");
    }

    private async Task<BarberBooking.Domain.Services.Service> GetBookableServiceAsync(
        Guid shopId, Guid barberId, Guid serviceId, CancellationToken ct)
    {
        if (!await _db.Shops.AnyAsync(x => x.Id == shopId && x.IsActive, ct) ||
            !await _db.Barbers.AnyAsync(x => x.Id == barberId && x.ShopId == shopId && x.IsActive, ct))
            throw new BookingException(404, "shop_or_barber_not_found");

        var service = await _db.Services.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == serviceId && x.ShopId == shopId && x.IsActive, ct);
        return service ?? throw new BookingException(404, "service_not_found");
    }

    public async Task<IReadOnlyList<BookingSlotDto>> GetAvailabilityAsync(
        Guid shopId, Guid barberId, Guid serviceId, DateOnly date, CancellationToken ct)
    {
        var service = await GetBookableServiceAsync(shopId, barberId, serviceId, ct);
        if (date < TodayLocal || date > TodayLocal.AddDays(30))
            return [];

        var localDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var dayStartUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localDay, _zone), TimeSpan.Zero);
        var dayEndUtc = new DateTimeOffset(
            TimeZoneInfo.ConvertTimeToUtc(localDay.AddDays(1), _zone), TimeSpan.Zero);

        var shifts = await _db.WeeklyShifts.AsNoTracking()
            .Where(s => s.BarberId == barberId && s.DayOfWeek == (int)localDay.DayOfWeek)
            .OrderBy(s => s.StartMinute)
            .ToListAsync(ct);

        var occupied = await _db.Appointments.AsNoTracking()
            .Where(a => a.BarberId == barberId && a.Status == AppointmentStatus.Confirmed &&
                a.StartUtc < dayEndUtc && a.EndUtc > dayStartUtc)
            .Select(a => new BookingSlotDto(a.StartUtc, a.EndUtc)).ToListAsync(ct);

        var timeOff = await _db.BarberTimeOffs.AsNoTracking()
            .Where(a => a.BarberId == barberId && a.StartUtc < dayEndUtc &&
                a.EndUtc > dayStartUtc)
            .Select(a => new BookingSlotDto(a.StartUtc, a.EndUtc)).ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var slots = new List<BookingSlotDto>();

        foreach (var shift in shifts)
        {
            for (var minute = shift.StartMinute;
                minute + service.DurationMinutes <= shift.EndMinute; minute += 15)
            {
                var localStart = localDay.AddMinutes(minute);
                var localEnd = localStart.AddMinutes(service.DurationMinutes);

                // Never silently resolve a non-existent or duplicated wall-clock time.
                if (_zone.IsInvalidTime(localStart) || _zone.IsInvalidTime(localEnd) ||
                    _zone.IsAmbiguousTime(localStart) || _zone.IsAmbiguousTime(localEnd))
                    continue;

                var start = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localStart, _zone), TimeSpan.Zero);
                var end = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localEnd, _zone), TimeSpan.Zero);
                if (end - start != TimeSpan.FromMinutes(service.DurationMinutes) ||
                    start < now.AddMinutes(15))
                    continue;

                if (occupied.Any(x => x.StartUtc < end && x.EndUtc > start) ||
                    timeOff.Any(x => x.StartUtc < end && x.EndUtc > start))
                    continue;

                slots.Add(new BookingSlotDto(start, end));
            }
        }

        return slots;
    }

    public async Task<BookingDto> BookAsync(Guid customerId, CreateBookingRequest request, CancellationToken ct)
    {
        if (request.StartUtc.Offset != TimeSpan.Zero)
            throw new BookingException(400, "start_must_be_utc");

        var service = await GetBookableServiceAsync(request.ShopId, request.BarberId, request.ServiceId, ct);
        var localStart = TimeZoneInfo.ConvertTime(request.StartUtc, _zone);
        var date = DateOnly.FromDateTime(localStart.Date);

        var available = await GetAvailabilityAsync(request.ShopId, request.BarberId,
            request.ServiceId, date, ct);

        if (!available.Any(x => x.StartUtc == request.StartUtc))
            throw new BookingException(409, "slot_unavailable");

        var appointment = Appointment.Create(request.ShopId, request.BarberId,
            request.ServiceId, customerId, request.StartUtc,
            service.DurationMinutes, service.Price.Amount, DateTimeOffset.UtcNow);

        _db.Appointments.Add(appointment);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.ExclusionViolation)
        {
            // The DB constraint is authoritative if requests race.
            throw new BookingException(409, "slot_unavailable");
        }
        return ToDto(appointment);
    }

    public async Task<IReadOnlyList<BookingDto>> ListMineAsync(Guid customerId, CancellationToken ct)
    {
        var data = await _db.Appointments.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.StartUtc)
            .Take(100).ToListAsync(ct);
        return data.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<BookingDto>> ListOwnerAsync(Guid ownerId, CancellationToken ct)
    {
        var data = await _db.Appointments.AsNoTracking()
            .Where(x => _db.Shops.Any(s => s.Id == x.ShopId && s.OwnerUserId == ownerId))
            .OrderByDescending(x => x.StartUtc)
            .Take(200).ToListAsync(ct);
        return data.Select(ToDto).ToList();
    }

    public async Task CancelAsync(Guid bookingId, Guid userId, bool isOwner, CancellationToken ct)
    {
        var booking = await _db.Appointments.SingleOrDefaultAsync(x => x.Id == bookingId, ct);
        if (booking is null)
            throw new BookingException(404, "booking_not_found");

        var authorized = isOwner
            ? await _db.Shops.AnyAsync(s => s.Id == booking.ShopId && s.OwnerUserId == userId, ct)
            : booking.CustomerId == userId;

        if (!authorized) throw new BookingException(404, "booking_not_found");
        if (booking.Status == AppointmentStatus.Cancelled) return;
        if (!isOwner && booking.StartUtc < DateTimeOffset.UtcNow.AddHours(2))
            throw new BookingException(409, "cancellation_window_closed");

        booking.Cancel(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<WeeklyShiftDto>> GetScheduleAsync(
        Guid ownerId, Guid barberId, CancellationToken ct)
    {
        await AssertOwnerBarberAsync(ownerId, barberId, ct);
        var shifts = await _db.WeeklyShifts.AsNoTracking()
            .Where(s => s.BarberId == barberId)
            .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartMinute).ToListAsync(ct);
        return shifts.Select(x => new WeeklyShiftDto(x.DayOfWeek,
            MinutesToTime(x.StartMinute), MinutesToTime(x.EndMinute))).ToList();
    }

    private static string MinutesToTime(int minute) =>
        minute == 1440 ? "24:00" : TimeSpan.FromMinutes(minute).ToString(@"hh\:mm", CultureInfo.InvariantCulture);

    private static int ParseMinute(string time)
    {
        if (time == "24:00") return 1440;
        if (!TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var value))
            throw new BookingException(400, "invalid_shift_time");
        return value.Hour * 60 + value.Minute;
    }

    public async Task SetScheduleAsync(Guid ownerId, Guid barberId,
        ScheduleRequest request, CancellationToken ct)
    {
        await AssertOwnerBarberAsync(ownerId, barberId, ct);
        if (request.Shifts is null || request.Shifts.Count > 21)
            throw new BookingException(400, "invalid_schedule");

        var shifts = request.Shifts.Select(x =>
            new WeeklyShift(barberId, x.DayOfWeek, ParseMinute(x.Start), ParseMinute(x.End))).ToList();

        foreach (var group in shifts.GroupBy(s => s.DayOfWeek))
        {
            var sorted = group.OrderBy(s => s.StartMinute).ToArray();
            if (sorted.Zip(sorted.Skip(1)).Any(x => x.First.EndMinute > x.Second.StartMinute))
                throw new BookingException(400, "overlapping_shifts");
        }

        // Do not invalidate previously accepted future appointments.
        var future = await _db.Appointments.AsNoTracking().Where(a =>
            a.BarberId == barberId && a.Status == AppointmentStatus.Confirmed &&
            a.StartUtc >= DateTimeOffset.UtcNow).ToListAsync(ct);
        foreach (var a in future)
        {
            var start = TimeZoneInfo.ConvertTime(a.StartUtc, _zone);
            var end = TimeZoneInfo.ConvertTime(a.EndUtc, _zone);
            var minute = start.Hour * 60 + start.Minute;
            var endMinute = end.Hour * 60 + end.Minute;
            if (start.Date != end.Date || !shifts.Any(s =>
                s.DayOfWeek == (int)start.DayOfWeek && s.StartMinute <= minute &&
                s.EndMinute >= endMinute))
                throw new BookingException(409, "schedule_conflicts_with_bookings");
        }

        var existing = await _db.WeeklyShifts.Where(s => s.BarberId == barberId).ToListAsync(ct);
        _db.WeeklyShifts.RemoveRange(existing);
        _db.WeeklyShifts.AddRange(shifts);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddTimeOffAsync(Guid ownerId, Guid barberId,
        TimeOffRequest request, CancellationToken ct)
    {
        await AssertOwnerBarberAsync(ownerId, barberId, ct);
        if (request.StartUtc < DateTimeOffset.UtcNow ||
            request.EndUtc <= request.StartUtc ||
            request.StartUtc.Offset != TimeSpan.Zero || request.EndUtc.Offset != TimeSpan.Zero)
            throw new BookingException(400, "invalid_time_off");

        if (await _db.Appointments.AnyAsync(a =>
            a.BarberId == barberId && a.Status == AppointmentStatus.Confirmed &&
            a.StartUtc < request.EndUtc && a.EndUtc > request.StartUtc, ct))
            throw new BookingException(409, "time_off_conflicts_with_bookings");

        _db.BarberTimeOffs.Add(new BarberTimeOff(barberId,
            request.StartUtc, request.EndUtc, request.Note));
        await _db.SaveChangesAsync(ct);
    }
}
