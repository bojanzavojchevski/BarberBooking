using BarberBooking.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Services;

public sealed class DeleteServiceUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IServiceRepository _services;
    private readonly IUnitOfWork _uow;

    public DeleteServiceUseCase(ICurrentUser currentUser, IShopRepository shops, IServiceRepository services, IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _shops = shops;
        _services = services;
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid serviceId, CancellationToken ct)
    {
        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var service = await _services.GetByIdAsync(serviceId, ct)
            ?? throw new InvalidOperationException("service_not_found");

        if (service.ShopId != shop.Id)
            throw new InvalidOperationException("service_not_found");

        service.SoftDelete(_currentUser.UserId, DateTimeOffset.UtcNow);

        await _uow.SaveChangesAsync(ct);
    }
}
