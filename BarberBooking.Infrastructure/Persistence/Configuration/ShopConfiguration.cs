using BarberBooking.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Persistence.Configuration;

public sealed class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> b)
    {
        b.ToTable("shops");

        b.HasKey(x => x.Id);

        b.Property(x => x.OwnerUserId).IsRequired();

        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(80).IsRequired();

        b.Property(x => x.IsActive).IsRequired();

        b.HasIndex(x => x.OwnerUserId).IsUnique();
        b.HasIndex(x => x.Slug).IsUnique();

        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
