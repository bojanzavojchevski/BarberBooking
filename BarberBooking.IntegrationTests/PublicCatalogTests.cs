using System.Net;
using System.Text.Json;
using BarberBooking.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarberBooking.IntegrationTests;

public sealed class PublicCatalogTests : IClassFixture<TestAppFactory>
{
    private readonly TestAppFactory _factory;

    public PublicCatalogTests(TestAppFactory factory) => _factory = factory;

    private async Task SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await CatalogSeeder.SeedAsync(db);
    }

    [Fact]
    public async Task GetPublicShops_ReturnsOnlyActiveShops()
    {
        await SeedAsync();
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/public/shops");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("downtown-cuts");
        json.Should().NotContain("night-shop"); // inactive shop hidden
    }

    [Fact]
    public async Task GetShopDetails_ReturnsOnlyActiveServicesAndNonDeletedBarbers()
    {
        await SeedAsync();
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/public/shops/downtown-cuts");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await res.Content.ReadAsStringAsync();

        // services: Fade active, Buzz inactive
        json.Should().Contain("\"name\":\"Fade\"");
        json.Should().NotContain("\"name\":\"Buzz\"");

        // barbers: BOJAN exists, Deleted Barber is soft-deleted so filtered out
        json.Should().Contain("\"displayName\":\"BOJAN\"");
        json.Should().NotContain("Deleted Barber");
    }

    [Fact]
    public async Task GetShopDetails_InactiveShop_Returns404()
    {
        await SeedAsync();
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/public/shops/night-shop");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPublicShops_SearchBySlug_Works()
    {
        await SeedAsync();
        var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/public/shops?q=downtown");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("downtown-cuts");
    }
}
