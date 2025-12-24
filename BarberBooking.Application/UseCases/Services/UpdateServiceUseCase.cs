using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Services;

public sealed class UpdateServiceUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IServiceRepository _services;
    private readonly IUnitOfWork _uow;


    public UpdateServiceUseCase(ICurrentUser currentUser, IShopRepository shops, IServiceRepository services, IUnitOfWork uos)
    {
        _currentUser = currentUser;
        _shops = shops;
        _services = services;
        _uow = uos;
    }

    public async Task<ServiceDto> ExecuteAsync(Guid serviceId, UpdateServiceRequest request, CancellationToken ct)
    {
        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var service = await _services.GetByIdAsync(serviceId, ct)
            ?? throw new InvalidOperationException("service_not_found");

        if (service.ShopId != shop.Id)
            throw new InvalidOperationException("service_not_found");


        // uniqueness check
        var normalized = request.Name.Trim().ToUpperInvariant();
        if(!string.Equals(service.Name.Trim().ToUpperInvariant(), normalized, StringComparison.Ordinal))
        {
            var exists = await _services.ExistsByNormalizedNameAsync(shop.Id, normalized, ct);
            if (exists) throw new InvalidOperationException("service_name_taken");
        }

        service.SetName(request.Name);
        service.SetDuration(request.DurationMinutes);
        service.SetPrice(Money.From(request.PriceAmount));

        await _uow.SaveChangesAsync();

        return new ServiceDto(service.Id, service.Name, service.DurationMinutes, service.Price.Amount, service.IsActive);

    }


}
