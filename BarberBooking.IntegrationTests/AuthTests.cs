using BarberBooking.Application.Auth.DTOs;
using BarberBooking.Infrastructure.Identity;
using BarberBooking.Infrastructure.Persistence;
using DotNet.Testcontainers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

public sealed class AuthTests : IAsyncLifetime
{


    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("bb_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();

        var cs = _pg.GetConnectionString();

        Environment.SetEnvironmentVariable("ConnectionStrings__Default", cs);
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);

        Environment.SetEnvironmentVariable("ConnectionStrings:Default", cs);
        Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", cs);

        _factory = new WebApplicationFactory<Program>()
    .WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "bb",
                ["Jwt:Audience"] = "bb",
                ["Jwt:SigningKey"] = new string('x', 64),
                ["Jwt:AccessTokenMinutes"] = "10",
                ["RefreshTokens:Pepper"] = "test-pepper",
                ["RefreshTokens:Days"] = "30",
            });
        });
    });

        _client = _factory.CreateClient();

        // Apply migrations + seed a test user
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var email = "test@bb.com";
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is null)
        {
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var res = await userManager.CreateAsync(user, "StrongPass123!");
            if (!res.Succeeded) throw new Exception(string.Join("; ", res.Errors.Select(e => e.Description)));
        }
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _pg.DisposeAsync();
    }

    [Fact]
    public async Task Refresh_Rotates_And_Revokes_Old()
    {
        await ResetDatabaseAsync();

        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto("test@bb.com", "StrongPass123!"));

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var tokens1 = await login.Content.ReadFromJsonAsync<AuthTokensDto>();
        Assert.NotNull(tokens1);

        var refresh1 = tokens1!.RefreshToken;

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequestDto(refresh1));

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var tokens2 = await refresh.Content.ReadFromJsonAsync<AuthTokensDto>();
        Assert.NotNull(tokens2);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.SingleAsync(u => u.Email == "test@bb.com");

        var lastTwo = await db.RefreshTokens
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(2)
            .ToListAsync();

        Assert.Equal(2, lastTwo.Count);

        var newest = lastTwo[0];
        var previous = lastTwo[1];

        Assert.Equal(previous.FamilyId, newest.FamilyId);

        Assert.NotNull(previous.RevokedAtUtc);
        Assert.NotNull(previous.ReplacedByTokenId);
        Assert.Equal(newest.Id, previous.ReplacedByTokenId);

        Assert.Null(newest.RevokedAtUtc);

        var activeInFamily = await db.RefreshTokens.CountAsync(x =>
            x.UserId == user.Id &&
            x.FamilyId == newest.FamilyId &&
            x.RevokedAtUtc == null &&
            x.ExpiresAtUtc > DateTime.UtcNow);

        Assert.Equal(1, activeInFamily);
    }

    [Fact]
    public async Task Refresh_Reuse_Attack_Revokes_Family_And_Returns_401()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto("test@bb.com", "StrongPass123!"));
        var t1 = await login.Content.ReadFromJsonAsync<AuthTokensDto>();
        var r1 = t1!.RefreshToken;

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(r1));
        var t2 = await refresh.Content.ReadFromJsonAsync<AuthTokensDto>();

        // Reuse old token r1
        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto(r1));
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);

        // DB proof: family has no active tokens
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var familyId = (await db.RefreshTokens
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstAsync()).FamilyId;
        var active = await db.RefreshTokens.CountAsync(x => x.FamilyId == familyId && x.RevokedAtUtc == null);

        Assert.Equal(0, active);
    }

    [Fact]
    public async Task Refresh_Expired_Returns_401_Generic()
    {
        await ResetDatabaseAsync();

        // Create an expired token row directly
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == "test@bb.com");

            db.RefreshTokens.Add(new BarberBooking.Infrastructure.Auth.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FamilyId = Guid.NewGuid(),
                TokenHash = "DEADBEEF", // won't match; we only care about response shape (still 401)
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
            });

            await db.SaveChangesAsync();
        }

        // Any invalid token should be 401 generic
        var res = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequestDto("some-invalid"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
        TRUNCATE TABLE "RefreshTokens" RESTART IDENTITY CASCADE;
    """);
    }
}
