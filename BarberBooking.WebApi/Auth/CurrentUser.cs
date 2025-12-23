using BarberBooking.Application.Interfaces;
using System.Security.Claims;



namespace BarberBooking.WebApi.Auth;
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public bool isAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var user = _http.HttpContext?.User;

            var idStr =
                user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user?.FindFirstValue("sub");

            return Guid.TryParse(idStr, out var id)
                ? id
                : throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");
        }
    }
}
