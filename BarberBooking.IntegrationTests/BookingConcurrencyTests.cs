using BarberBooking.Application.Bookings;
using BarberBooking.Domain.Barbers;
using BarberBooking.Domain.Services;
using BarberBooking.Domain.Shops;
using BarberBooking.Domain.ValueObjects;
using BarberBooking.Infrastructure.Identity;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BarberBooking.IntegrationTests;

public sealed class BookingConcurrencyTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;
    public BookingConcurrencyTests(TestAppFactory factory) => _factory = factory;

    [Fact]
    public async Task TwoCustomersCannotBookTheSameBarberAtTheSameTime()
    {
        var ownerId = Guid.NewGuid();
        Guid shopId, barberId, serviceId, customer1, customer2;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var a = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = $"book-{Guid.NewGuid():N}@example.test",
                UserName = $"book-{Guid.NewGuid():N}@example.test",
                EmailConfirmed = true
            };
            var b = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = $"book-{Guid.NewGuid():N}@example.test",
                UserName = $"book-{Guid.NewGuid():N}@example.test",
                EmailConfirmed = true
            };
            Assert.True((await users.CreateAsync(a, "GoodPassword123!")).Succeeded);
            Assert.True((await users.CreateAsync(b, "GoodPassword123!")).Succeeded);
            customer1 = a.Id; customer2 = b.Id;

            var shop = new Shop(ownerId, "Concurrent Cuts", "concurrent-" + Guid.NewGuid().ToString("N"));
            var barber = new Barber(shop.Id, "Test Barber", "Booking test");
            var service = new Service(shop.Id, "Haircut", 30, Money.From(350));

            db.Shops.Add(shop);
            db.Barbers.Add(barber);
            db.Services.Add(service);
            await db.SaveChangesAsync();

            shopId = shop.Id; barberId = barber.Id; serviceId = service.Id;
        }

        // Find the next weekday date with a supported 09:00 local slot.
        using (var scope = _factory.Services.CreateScope())
        {
            var booking = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await booking.SetScheduleAsync(ownerId, barberId, new ScheduleRequest(
                Enumerable.Range(0, 7)
                    .Select(d => new WeeklyShiftDto(d, "09:00", "17:00")).ToArray()),
                CancellationToken.None);
        }

        var target = Enumerable.Range(2, 10)
            .Select(offset => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(offset)))
            .First();
        DateTimeOffset start;
        using (var scope = _factory.Services.CreateScope())
        {
            var booking = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var slots = await booking.GetAvailabilityAsync(shopId, barberId, serviceId, target,
                CancellationToken.None);
            Assert.NotEmpty(slots);
            start = slots[0].StartUtc;
        }

        async Task<string> Attempt(Guid user)
        {
            using var scope = _factory.Services.CreateScope();
            var booking = scope.ServiceProvider.GetRequiredService<IBookingService>();
            try
            {
                await booking.BookAsync(user,
                    new CreateBookingRequest(shopId, barberId, serviceId, start),
                    CancellationToken.None);
                return "booked";
            }
            catch (BookingException e) when (e.StatusCode == 409)
            {
                return "conflict";
            }
        }

        var results = await Task.WhenAll(Attempt(customer1), Attempt(customer2));
        Assert.Equal(1, results.Count(x => x == "booked"));
        Assert.Equal(1, results.Count(x => x == "conflict"));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(1, db.Appointments.Count(x =>
                x.BarberId == barberId && x.StartUtc == start));
        }
    }
}
