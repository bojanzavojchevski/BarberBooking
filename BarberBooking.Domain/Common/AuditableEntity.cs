using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Domain.Common;

public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public void SetCreated(Guid userId, DateTimeOffset nowUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        CreatedByUserId = userId;
        CreatedAtUtc = nowUtc;
    }

    public void SetUpdated(Guid userId, DateTimeOffset nowUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        UpdatedByUserId = userId;
        UpdatedAtUtc = nowUtc;
    }
}

