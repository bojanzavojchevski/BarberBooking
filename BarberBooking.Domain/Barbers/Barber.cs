using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarberBooking.Domain.Common;

namespace BarberBooking.Domain.Barbers;

public sealed class Barber : AuditableEntity, ISoftDeletable
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShopId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;
    public string? Bio { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    private Barber() { }

    public Barber(Guid shopId, string displayName, string? bio)
    {
        if (shopId == Guid.Empty) throw new ArgumentException("ShopId is required.", nameof(shopId));
        ShopId = shopId;

        SetDisplayName(displayName);
        SetBio(bio);
        IsActive = true;
    }

    public void SetDisplayName(string displayName)
    {
        displayName = (displayName ?? string.Empty).Trim();
        if (displayName.Length < 2) throw new ArgumentException("DisplayName is too short.", nameof(displayName));
        if (displayName.Length > 120) throw new ArgumentException("DisplayName is too long.", nameof(displayName));
        DisplayName = displayName;
    }

    public void SetBio(string? bio)
    {
        bio = bio?.Trim();
        if (bio is { Length: > 600 }) throw new ArgumentException("Bio is too long.", nameof(bio));
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void SoftDelete(Guid userId, DateTimeOffset nowUtc)
    {
        if (IsDeleted) return;
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));

        IsDeleted = true;
        DeletedByUserId = userId;
        DeletedAtUtc = nowUtc;
        SetUpdated(userId, nowUtc);
    }
}

