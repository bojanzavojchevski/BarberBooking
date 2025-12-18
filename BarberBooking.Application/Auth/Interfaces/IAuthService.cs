using BarberBooking.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthTokensDto> LoginAsync(LoginRequestDto request, ClientContextDto client, CancellationToken ct);
    Task<AuthTokensDto> RefreshAsync(RefreshRequestDto request, ClientContextDto client, CancellationToken ct);
}
