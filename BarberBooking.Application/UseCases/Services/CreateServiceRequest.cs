using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.UseCases.Services;

public sealed record CreateServiceRequest(
    string Name,
    int DurationMinutes,
    decimal PriceAmount
    );
