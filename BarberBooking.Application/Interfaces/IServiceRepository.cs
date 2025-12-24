using BarberBooking.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Interfaces;

public interface IServiceRepository
{
    Task AddAsync(Service service, CancellationToken ct);
    Task<Service?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Service>> ListByShopIdAsync(Guid shopId, CancellationToken ct);
    Task<bool> ExistsByNormalizedNameAsync(Guid shopId, string normalizedName, CancellationToken ct);
}
