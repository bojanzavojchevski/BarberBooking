using BarberBooking.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Barbers;

public sealed class ListMyBarbersUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IBarberRepository _barbers;

    public ListMyBarbersUseCase(ICurrentUser currentUser, IShopRepository shops, IBarberRepository barbers)
    {
        _currentUser = currentUser;
        _shops = shops;
        _barbers = barbers;
    }

    public async Task<IReadOnlyCollection<BarberSummaryDto>> ExecuteAsync(CancellationToken ct)
    {
        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var list = await _barbers.ListByShopIdAsync(shop.Id, ct);


        return list.Select(b => new BarberSummaryDto(
            b.Id, b.DisplayName, b.Bio, b.IsActive
            )).ToList();

    }

}