using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarberBooking.Domain.Exceptions;

namespace BarberBooking.Domain.ValueObjects;

public sealed record TimeRange
{
    public DateTimeOffset StartUtc { get; }
    public DateTimeOffset EndUtc { get; }

    public TimeRange(DateTimeOffset startUtc, DateTimeOffset endUtc)
    {
        if (endUtc <= startUtc)
            throw new DomainException("End must be after start");

        StartUtc = startUtc;
        EndUtc = endUtc;
    }
}
