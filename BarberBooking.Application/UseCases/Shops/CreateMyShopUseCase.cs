using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Shops;

public sealed class CreateMyShopUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IUnitOfWork _uow;

    public CreateMyShopUseCase(ICurrentUser currentUser, IShopRepository shops, IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _shops = shops;
        _uow = uow;
    }

    public async Task<ShopDto> ExecuteAsync(CreateMyShopRequest request,  CancellationToken ct = default)
    {
        var ownerId = _currentUser.UserId;

        var existing = await _shops.GetByOwnerUserIdAsync(ownerId, ct);
        if (existing is not null)
            throw new InvalidOperationException("Owner already has a shop.");

        var shop = new Shop(ownerId, request.Name, request.Slug);
        shop.SetCreated(ownerId, DateTimeOffset.UtcNow);

        await _shops.AddAsync(shop, ct);
        await _uow.SaveChangesAsync();

        return new ShopDto(shop.Id, shop.Name, shop.Slug, shop.IsActive);
    }

}
