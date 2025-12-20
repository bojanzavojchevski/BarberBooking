using BarberBooking.Application.Auth.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth;

public sealed class SecurityEventLogger : ISecurityEventLogger
{
    private readonly ILogger _log;

    public SecurityEventLogger()
        => _log = Log.ForContext<SecurityEventLogger>();

    public void RefreshTokenReuseDetected(Guid userId, Guid familyId, string? ip, string? userAgent, string traceId)
    {
        _log.Warning("security.refresh_token_reuse_detected userId={UserId} familyId={FamilyId} ip={Ip} ua={UserAgent} traceId={TraceId}",
            userId, familyId, ip, userAgent, traceId);
    }


}
