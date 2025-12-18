using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.Interfaces;

public interface IUserAuthProvider
{
    Task<(Guid userId, string Email)?> FindByEmailAsync(string email, CancellationToken ct);
    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken ct);
    Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken ct);
    Task<string?> GetEmailByIdAsync(Guid userId, CancellationToken ct);

}
