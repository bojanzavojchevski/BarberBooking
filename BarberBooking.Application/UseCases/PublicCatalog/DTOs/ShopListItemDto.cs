using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.PublicCatalog.DTOs;

public sealed record ShopListItemDto(
    Guid Id,
    string Name,
    string Slug
);


