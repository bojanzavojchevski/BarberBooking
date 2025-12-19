using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Auth;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid FamilyId { get; set; }

    public string TokenHash { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? UserAgent { get; set; }

    //public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
