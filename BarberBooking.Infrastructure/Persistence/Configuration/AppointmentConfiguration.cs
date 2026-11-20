using BarberBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberBooking.Infrastructure.Persistence.Configuration;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.ToTable("appointments");
        b.HasKey(x => x.Id);
        b.Property(x => x.StartUtc).IsRequired();
        b.Property(x => x.EndUtc).IsRequired();
        b.Property(x => x.PriceAmount).HasPrecision(12, 2);
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => new { x.CustomerId, x.StartUtc });
        b.HasIndex(x => new { x.BarberId, x.StartUtc });
    }
}
