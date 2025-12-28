using BarberBooking.Application.DTOs;
using BarberBooking.Application.UseCases.PublicCatalog.DTOs;
using BarberBooking.Application.UseCases.PublicCatalog.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Interfaces;

public interface IPublicCatalogReadService
{
    Task<PagedResult<ShopListItemDto>> GetShopsAsync(GetPublicShopsQuery query, CancellationToken ct);
    Task<ShopPublicDetailsDto?> GetShopBySlugAsync(string slug, CancellationToken ct);
}
