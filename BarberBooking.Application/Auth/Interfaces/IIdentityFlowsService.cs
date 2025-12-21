using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.Interfaces;

public interface IIdentityFlowsService
{
    Task RequestEmailConfirmationAsync(string email, CancellationToken ct);
    Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct);

    Task RequestPasswordResetAsync(string email, CancellationToken ct);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct);
}
