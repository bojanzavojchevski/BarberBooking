using BarberBooking.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Persistence.Configuration;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.TokenHash)
            .IsRequired();

        b.HasIndex(x => x.TokenHash)
            .IsUnique();

        b.HasIndex(x => x.FamilyId);

        b.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.ExpiresAtUtc).IsRequired();
    }
}
