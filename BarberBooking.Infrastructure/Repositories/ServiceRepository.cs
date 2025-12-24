using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.Services;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Repositories;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _db;

    public ServiceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Service service, CancellationToken ct)
    {
        await _db.AddAsync(service, ct);
    }

    public Task<bool> ExistsByNormalizedNameAsync(Guid shopId, string normalizedName, CancellationToken ct)
    {
        return _db.Services
            .AsNoTracking()
            .AnyAsync(s =>
                s.ShopId == shopId &&
                EF.Property<string>(s, "normalized_name") == normalizedName,
                ct);
    }

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Services.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Service>> ListByShopIdAsync(Guid shopId, CancellationToken ct)
    {
        var list = await _db.Services
            .AsNoTracking()
            .Where(s => s.ShopId == shopId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return list;
    }
}
