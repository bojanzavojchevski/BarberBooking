using BarberBooking.Domain.Barbers;
using BarberBooking.Domain.Services;
using BarberBooking.Domain.Shops;
using BarberBooking.Domain.ValueObjects;
using BarberBooking.Infrastructure.Persistence;

namespace BarberBooking.IntegrationTests;

public static class CatalogSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Clean tables to keep tests deterministic
        db.Barbers.RemoveRange(db.Barbers);
        db.Services.RemoveRange(db.Services);
        db.Shops.RemoveRange(db.Shops);
        await db.SaveChangesAsync();

        var ownerA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var ownerB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var shopA = new Shop(ownerA, "Downtown Cuts", "downtown-cuts");
        var shopB = new Shop(ownerB, "Night Shop", "night-shop");
        shopB.SetActive(false);

        db.Shops.AddRange(shopA, shopB);
        await db.SaveChangesAsync();

        var fade = new Service(shopA.Id, "Fade", 30, Money.From(300));
        var buzz = new Service(shopA.Id, "Buzz", 15, Money.From(150));
        buzz.SetActive(false);

        db.Services.AddRange(fade, buzz);

        var bojan = new Barber(shopA.Id, "BOJAN", "Updated bio");
        var deleted = new Barber(shopA.Id, "Deleted Barber", null);
        deleted.SoftDelete(ownerA, DateTimeOffset.UtcNow);

        db.Barbers.AddRange(bojan, deleted);

        await db.SaveChangesAsync();
    }
}
