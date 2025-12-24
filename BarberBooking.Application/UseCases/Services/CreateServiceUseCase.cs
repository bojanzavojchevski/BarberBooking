using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.Services;
using BarberBooking.Domain.Shops;
using BarberBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Services;

public sealed class CreateServiceUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IServiceRepository _services;
    private readonly IUnitOfWork _uow;


    public CreateServiceUseCase(ICurrentUser currentUser, IShopRepository shops, IServiceRepository services, IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _shops = shops;
        _services = services;
        _uow = uow;
    }

    public async Task<ServiceDto> ExecuteAsync(CreateServiceRequest request, CancellationToken ct)
    {
        if (!_currentUser.isAuthenticated)
            throw new InvalidOperationException("unauthenticated");

        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var normalized = (request.Name ?? string.Empty).Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("invalid_service_name");

        if (await _services.ExistsByNormalizedNameAsync(shop.Id, normalized, ct))
            throw new InvalidOperationException("service_name_taken");

        var service = new Service(
            shop.Id,
            request.Name,
            request.DurationMinutes,
            Money.From(request.PriceAmount)
        );

        await _services.AddAsync(service, ct);
        await _uow.SaveChangesAsync(ct);

        return new ServiceDto(
            service.Id,
            service.Name,
            service.DurationMinutes,
            service.Price.Amount,
            service.IsActive
        );
    }
}
