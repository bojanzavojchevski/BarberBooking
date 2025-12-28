using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.PublicCatalog.Queries;

public sealed record GetPublicShopsQuery(
    string? Q,
    int Page = 1,
    int PageSize = 20
);


