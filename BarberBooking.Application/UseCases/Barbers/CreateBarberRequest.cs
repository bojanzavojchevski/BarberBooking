using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Barbers;

public sealed record CreateBarberRequest(
    string DisplayName,
    string? Bio
);
