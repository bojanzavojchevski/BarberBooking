using BarberBooking.Domain.Exceptions;
using BarberBooking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid BarberId { get; set; }
    public Guid CustomerId { get; set; }
    public required TimeRange TimeRange { get; set; }


    Appointment()
    {
    }

    public static Appointment Create(
        Guid barberId,
        Guid customerId,
        TimeRange range,
        DateTimeOffset nowUtc)
    {
        if (range.StartUtc < nowUtc)
        {
            throw new DomainException("Cannot book an appointment in the past");
        }

        return new Appointment()
        {
            Id = barberId,
            BarberId = barberId,
            CustomerId = customerId,
            TimeRange = range,
        };
    }
}
