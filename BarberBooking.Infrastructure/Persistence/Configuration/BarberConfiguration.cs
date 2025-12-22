using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarberBooking.Domain.Barbers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberBooking.Infrastructure.Persistence.Configurations;

public sealed class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> b)
    {
        b.ToTable("barbers");

        b.HasKey(x => x.Id);

        b.Property(x => x.ShopId).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
        b.Property(x => x.Bio).HasMaxLength(600);
        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => new { x.ShopId, x.IsActive });

        b.Property<string>("normalized_display_name")
            .HasMaxLength(120)
            .IsRequired();

        b.HasIndex("ShopId", "normalized_display_name").IsUnique();

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

