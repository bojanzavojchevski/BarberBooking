using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.Interfaces;

public interface ISecurityEventLogger
{
    void RefreshTokenReuseDetected(Guid userId, Guid familyId, string? ip, string? userAgent, string traceId);
}
