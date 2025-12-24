using BarberBooking.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Services;

public sealed class ListMyServicesUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IServiceRepository _services;

    public ListMyServicesUseCase(ICurrentUser currentUser, IShopRepository shops, IServiceRepository services)
    {
        _currentUser = currentUser;
        _shops = shops;
        _services = services;
    }


    public async Task<IReadOnlyList<ServiceSummaryDto>> ExecuteAsync(CancellationToken ct)
    {
        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var list = await _services.ListByShopIdAsync(shop.Id, ct);


        return list.Select(s => new ServiceSummaryDto(
            s.Id, s.Name, s.DurationMinutes, s.Price.Amount, s.IsActive
            )).ToList();
    }


}
