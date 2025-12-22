using BarberBooking.Domain.Common;

namespace BarberBooking.Domain.Shops;

public sealed class Shop : AuditableEntity, ISoftDeletable
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid OwnerUserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    private Shop() { }

    public Shop(Guid ownerUserId, string name, string slug)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("OwnerUserId is required.", nameof(ownerUserId));

        SetName(name);
        SetSlug(slug);

        OwnerUserId = ownerUserId;
        IsActive = true;
    }

    public void SetName(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length < 2)
            throw new ArgumentException("Shop name is too short.", nameof(name));
        if (name.Length > 120)
            throw new ArgumentException("Shop name is too long.", nameof(name));
        Name = name;
    }

    public void SetSlug(string slug)
    {
        slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (slug.Length < 3)
            throw new ArgumentException("Slug is too short.", nameof(slug));
        if (slug.Length > 80)
            throw new ArgumentException("Slug is too long.", nameof(slug));
        if (slug.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-')))
            throw new ArgumentException("Slug contains invalid characters.", nameof(slug));
        Slug = slug;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void SoftDelete(Guid userId, DateTimeOffset nowUtc)
    {
        if (IsDeleted) return;
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        IsDeleted = true;
        DeletedByUserId = userId;
        DeletedAtUtc = nowUtc;
        SetUpdated(userId, nowUtc);
    }
}
