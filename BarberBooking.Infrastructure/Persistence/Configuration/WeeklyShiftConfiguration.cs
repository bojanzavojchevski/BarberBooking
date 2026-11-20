using BarberBooking.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberBooking.Infrastructure.Persistence.Configuration;

public sealed class WeeklyShiftConfiguration : IEntityTypeConfiguration<WeeklyShift>
{
    public void Configure(EntityTypeBuilder<WeeklyShift> b)
    {
        b.ToTable("weekly_shifts");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.BarberId, x.DayOfWeek });
    }
}
