using BarberBooking.Application.Interfaces;
using BarberBooking.Domain.Barbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Barbers;

public sealed class CreateBarberUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IBarberRepository _barbers;
    private readonly IUnitOfWork _uow;

    public CreateBarberUseCase(ICurrentUser currentUser, IShopRepository shops, IBarberRepository barbers, IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _shops = shops;
        _barbers = barbers;
        _uow = uow;
    }

    public async Task<BarberDto> ExecuteAsync(CreateBarberRequest request, CancellationToken ct)
    {
        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var normalized = request.DisplayName.Trim().ToUpperInvariant();

        var exists = await _barbers.ExistsByNormalizedDisplayNameAsync(shop.Id, normalized, ct);
        if (exists) throw new InvalidOperationException("barber_name_taken");

        var barber = new Barber(shop.Id, request.DisplayName, request.Bio);

        await _barbers.AddAsync(barber, ct);
        await _uow.SaveChangesAsync();

        return new BarberDto(barber.Id, barber.DisplayName, barber.Bio, barber.IsActive);

    }
}
