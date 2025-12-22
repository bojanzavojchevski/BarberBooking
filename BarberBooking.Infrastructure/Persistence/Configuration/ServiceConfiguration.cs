using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarberBooking.Domain.Services;
using Microsoft.EntityFrameworkCore;
using BarberBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberBooking.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> b)
    {
        b.ToTable("services");

        b.HasKey(x => x.Id);

        b.Property(x => x.ShopId).IsRequired();
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.DurationMinutes).IsRequired();
        b.Property(x => x.IsActive).IsRequired();

        
        b.Property(x => x.Price)
            .HasConversion(
            v => v.Amount,
            v => Money.From(v))
            .HasColumnName("price_amount")
            .HasPrecision(12, 2)
            .IsRequired();

        b.HasIndex(x => new { x.ShopId, x.IsActive });


        b.Property<string>("normalized_name")
            .HasMaxLength(120)
            .IsRequired();

        b.HasIndex("ShopId", "normalized_name").IsUnique();

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

