using BarberBooking.Application.DTOs;
using BarberBooking.Application.Interfaces;
using BarberBooking.Application.UseCases.PublicCatalog.DTOs;
using BarberBooking.Application.UseCases.PublicCatalog.Queries;
using BarberBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Catalog;

public sealed class PublicCatalogReadService : IPublicCatalogReadService
{
    private readonly AppDbContext _db;

    public PublicCatalogReadService(AppDbContext db) => _db = db;


    public async Task<PagedResult<ShopListItemDto>> GetShopsAsync(GetPublicShopsQuery query, CancellationToken ct)
    {
        var q = _db.Shops
            .AsNoTracking()
            .Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            q = q.Where(s =>
                EF.Functions.ILike(s.Name, $"%{term}%") ||
                EF.Functions.ILike(s.Slug, $"%{term}%"));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(s => s.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new ShopListItemDto(
                s.Id,
                s.Name,
                s.Slug
            ))
            .ToListAsync(ct);

        return new PagedResult<ShopListItemDto>(items, total, query.Page, query.PageSize);
    }


    public async Task<ShopPublicDetailsDto?> GetShopBySlugAsync(string slug, CancellationToken ct)
    {
        var shop = await _db.Shops
            .AsNoTracking()
            .Where(s => s.IsActive && s.Slug == slug)
            .Select(s => new ShopPublicDto(s.Id, s.Name, s.Slug))
            .SingleOrDefaultAsync(ct);

        if (shop is null) return null;

        var services = await _db.Services
            .AsNoTracking()
            .Where(x => x.ShopId == shop.Id && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ServicePublicDto(
                x.Id,
                x.Name,
                x.DurationMinutes,
                x.Price.Amount
            ))
            .ToListAsync(ct);

        var barbers = await _db.Barbers
            .AsNoTracking()
            .Where(x => x.ShopId == shop.Id && x.IsActive)
            .OrderBy(x => x.DisplayName)
            .Select(x => new BarberPublicDto(
                x.Id,
                x.DisplayName,
                x.Bio
            ))
            .ToListAsync(ct);

        return new ShopPublicDetailsDto(shop, services, barbers);
    }

}
