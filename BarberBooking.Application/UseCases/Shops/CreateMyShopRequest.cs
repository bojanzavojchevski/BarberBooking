using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Shops;

public sealed record CreateMyShopRequest(string Name, string Slug);