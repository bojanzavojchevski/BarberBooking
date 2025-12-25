using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Barbers;

public sealed record BarberSummaryDto(
    Guid Id,
    string DisplayName,
    string? Bio,
    bool IsActive
);
