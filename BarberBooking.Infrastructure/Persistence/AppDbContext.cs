using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BarberBooking.Infrastructure.Identity;
using BarberBooking.Infrastructure.Auth;
using BarberBooking.Domain.Services;
using BarberBooking.Domain.Shops;
using BarberBooking.Domain.Barbers;

namespace BarberBooking.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Service> Services => Set<Service>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Barber> Barbers => Set<Barber>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TokenHash).IsUnique();

            b.Property(x => x.TokenHash).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.ExpiresAtUtc).IsRequired();

            b.Property(x => x.FamilyId).IsRequired();
            b.HasIndex(x => x.FamilyId);

            b.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
        });
    }

    public override int SaveChanges()
    {
        NormalizeCatalogFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        NormalizeCatalogFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void NormalizeCatalogFields()
    {
        foreach (var entry in ChangeTracker.Entries<Service>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            entry.Property("normalized_name").CurrentValue =
                (entry.Entity.Name ?? string.Empty).Trim().ToUpperInvariant();
        }
        foreach (var entry in ChangeTracker.Entries<Barber>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            entry.Property("normalized_display_name").CurrentValue =
                (entry.Entity.DisplayName ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
