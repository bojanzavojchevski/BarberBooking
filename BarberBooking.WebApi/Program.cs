using BarberBooking.Application.Auth;
using BarberBooking.Application.Auth.Interfaces;
using BarberBooking.Application.Interfaces;
using BarberBooking.Application.UseCases.Services;
using BarberBooking.Application.UseCases.Shops;
using BarberBooking.Infrastructure.DependencyInjection;
using BarberBooking.Infrastructure.Identity;
using BarberBooking.Infrastructure.Persistence;
using BarberBooking.WebApi.Auth;
using BarberBooking.WebApi.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        var signingKey = jwt["SigningKey"]!;

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(signingKey)
            ),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
{
    // strict role-only policies
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("OwnerOnly", p => p.RequireRole("Owner"));
    options.AddPolicy("BarberOnly", p => p.RequireRole("Barber"));
    options.AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));

    // least-privilege
    options.AddPolicy("CanManageShops", p => p.RequireRole("Admin", "Owner"));
    options.AddPolicy("CanManageAppointments", p => p.RequireRole("Admin", "Barber"));
    options.AddPolicy("CanBook", p => p.RequireRole("Admin", "Customer"));
});

builder.Services.AddScoped<IAuthService, AuthService>();


// HealthChecks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("db");

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddDataProtection();

// Rate Limiter
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.ForwardLimit = 1;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("""{"error":"too_many_requests"}""", token);
    };

    // /api/auth/login
    options.AddPolicy("auth-login-ip", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    // /api/auth/refresh
    options.AddPolicy("auth-refresh-ip", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<CreateMyShopUseCase>();
builder.Services.AddScoped<GetMyShopUseCase>();
builder.Services.AddScoped<CreateServiceUseCase>();
builder.Services.AddScoped<ListMyServicesUseCase>();
builder.Services.AddScoped<UpdateServiceUseCase>();
builder.Services.AddScoped<SetServiceActiveUseCase>();
builder.Services.AddScoped<DeleteServiceUseCase>();





var app = builder.Build();


app.UseForwardedHeaders();
app.UseRateLimiter();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

await RoleSeeder.SeedAsync(app.Services);

// Initialize Serilog 
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



//
app.UseMiddleware<ExceptionHandlingMiddleware>();

//
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();




public partial class Program { }