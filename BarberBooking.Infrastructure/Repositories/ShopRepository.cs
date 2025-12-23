using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.Shops;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Repositories;

public sealed class ShopRepository : IShopRepository
{
    private readonly AppDbContext _db;

    public ShopRepository(AppDbContext db) => _db = db;

    public Task<Shop?> GetByOwnerUserIdAsync(Guid ownerUserId, CancellationToken ct = default)
        => _db.Shops.FirstOrDefaultAsync(s => s.OwnerUserId == ownerUserId, ct);


    public Task AddAsync(Shop shop, CancellationToken ct = default)
    {
        _db.Shops.Add(shop);
        return Task.CompletedTask;
    }
}
