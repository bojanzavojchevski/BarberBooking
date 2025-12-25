using BarberBooking.Domain.Barbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Interfaces;

public interface IBarberRepository
{
    Task AddAsync(Barber barber, CancellationToken ct);
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Barber>> ListByShopIdAsync(Guid shopId, CancellationToken ct);
    Task<bool> ExistsByNormalizedDisplayNameAsync(Guid shopId, string normalizedDisplayName, CancellationToken ct);
}
