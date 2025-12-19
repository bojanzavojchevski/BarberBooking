using System.Net;
using System.Text.Json;

namespace BarberBooking.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/problem+json";

            var payload = new
            {
                type = "https://httpstatuses.com/401",
                title = "Unauthorized",
                status = 401,
                traceId = context.TraceIdentifier,
                detail = "Unauthorized"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var payload = new
            {
                type = "https://httpstatuses.com/500",
                title = "Internal Server Error",
                status = 500,
                traceId = context.TraceIdentifier,
                detail = "An unexpected error occurred."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
