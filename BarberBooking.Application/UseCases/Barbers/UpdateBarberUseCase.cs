using BarberBooking.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Barbers;

public sealed class UpdateBarberUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IShopRepository _shops;
    private readonly IBarberRepository _barbers;
    private readonly IUnitOfWork _uow;

    public UpdateBarberUseCase(ICurrentUser currentUser, IShopRepository shops, IBarberRepository barbers, IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _shops = shops;
        _barbers = barbers;
        _uow = uow;
    }

    public async Task<BarberDto> ExecuteAsync(Guid barberId, UpdateBarberRequest request, CancellationToken ct)
    {
        var shop = await _shops.GetByOwnerUserIdAsync(_currentUser.UserId, ct)
            ?? throw new InvalidOperationException("owner_has_no_shop");

        var barber = await _barbers.GetByIdAsync(barberId, ct)
            ?? throw new InvalidOperationException("barber_not_found");

        if (barber.ShopId != shop.Id)
            throw new InvalidOperationException("barber_not_found");

        var incomingNormalized = request.DisplayName.Trim().ToUpperInvariant();
        var currentNormalized = barber.DisplayName.Trim().ToUpperInvariant();

        if (!string.Equals(incomingNormalized, currentNormalized, StringComparison.Ordinal))
        {
            var exists = await _barbers.ExistsByNormalizedDisplayNameAsync(shop.Id, incomingNormalized, ct);
            if (exists) throw new InvalidOperationException("barber_name_taken");
        }

        barber.SetDisplayName(request.DisplayName);
        barber.SetBio(request.Bio);

        await _uow.SaveChangesAsync(ct);

        return new BarberDto(barber.Id, barber.DisplayName, barber.Bio, barber.IsActive);


    }

}
