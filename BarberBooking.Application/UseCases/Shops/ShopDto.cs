using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Shops;

public sealed record ShopDto(Guid Id, string Name, string Slug, bool IsActive);