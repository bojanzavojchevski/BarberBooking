using BarberBooking.Domain.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Interfaces;

public interface IShopRepository
{
    Task<Shop?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default);
    Task AddAsync(Shop shop, CancellationToken ct = default);
}
