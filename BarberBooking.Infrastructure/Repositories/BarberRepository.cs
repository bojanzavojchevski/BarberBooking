using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.Barbers;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Repositories;

public sealed class BarberRepository : IBarberRepository
{
    private readonly AppDbContext _db;

    public BarberRepository(AppDbContext db) => _db = db;

    public Task AddAsync(Barber barber, CancellationToken ct)
    {
        _db.Barbers.Add(barber);
        return Task.CompletedTask;
    }

    public async Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Barbers.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<Barber>> ListByShopIdAsync(Guid shopId, CancellationToken ct)
    {
        return await _db.Barbers
            .Where(b => b.ShopId == shopId)
            .OrderBy(b => b.DisplayName)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNormalizedDisplayNameAsync(Guid shopId, string normalizedDisplayName, CancellationToken ct)
    {
        return await _db.Barbers
            .AnyAsync(
            b => b.ShopId == shopId && EF.Property<string>(b, "normalized_display_name") == normalizedDisplayName, ct);
    }
}
