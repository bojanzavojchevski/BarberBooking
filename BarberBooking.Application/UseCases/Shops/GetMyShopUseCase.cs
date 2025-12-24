using BarberBooking.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Shops;

public sealed class GetMyShopUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;

    public GetMyShopUseCase(ICurrentUser currentUser, IShopRepository shops)
    {
        _currentUser = currentUser;
        _shops = shops;
    }

    public async Task<ShopDto?> ExecuteAsync(CancellationToken ct = default)
    {
        var ownerId = _currentUser.UserId;

        var shop = await _shops.GetByOwnerUserIdAsync(ownerId, ct);

        return shop is null
            ? null
            : new ShopDto(shop.Id, shop.Name, shop.Slug, shop.IsActive);
    }
}
