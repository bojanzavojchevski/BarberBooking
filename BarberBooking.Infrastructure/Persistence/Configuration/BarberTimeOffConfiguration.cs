using BarberBooking.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberBooking.Infrastructure.Persistence.Configuration;

public sealed class BarberTimeOffConfiguration : IEntityTypeConfiguration<BarberTimeOff>
{
    public void Configure(EntityTypeBuilder<BarberTimeOff> b)
    {
        b.ToTable("barber_time_off");
        b.HasKey(x => x.Id);
        b.Property(x => x.Note).HasMaxLength(250);
        b.HasIndex(x => new { x.BarberId, x.StartUtc });
    }
}
