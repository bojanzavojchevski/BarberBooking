using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Domain.ValueObjects;

public readonly record struct Money(decimal Amount)
{
    public static Money From(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");

        var rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        return new Money(rounded);
    }

    public override string ToString() => $"{Amount:0.00} MKD";

}
