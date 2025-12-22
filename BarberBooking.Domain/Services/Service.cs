using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarberBooking.Domain.Common;
using BarberBooking.Domain.ValueObjects;

namespace BarberBooking.Domain.Services;

public sealed class Service : AuditableEntity, ISoftDeletable
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ShopId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public Money Price { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    private Service() { }

    public Service(Guid shopId, string name, int durationMinutes, Money price)
    {
        if (shopId == Guid.Empty) throw new ArgumentException("ShopId is required.", nameof(shopId));

        ShopId = shopId;
        SetName(name);
        SetDuration(durationMinutes);
        Price = price;
        IsActive = true;
    }

    public void SetName(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length < 2) throw new ArgumentException("Service name is too short.", nameof(name));
        if (name.Length > 120) throw new ArgumentException("Service name is too long.", nameof(name));
        Name = name;
    }

    public void SetDuration(int minutes)
    {
        if (minutes < 5 || minutes > 480) throw new ArgumentOutOfRangeException(nameof(minutes), "Duration must be 5..480 minutes.");
        if (minutes % 5 != 0) throw new ArgumentException("Duration must be a multiple of 5 minutes.", nameof(minutes));
        DurationMinutes = minutes;
    }

    public void SetPrice(Money price) => Price = price;
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

