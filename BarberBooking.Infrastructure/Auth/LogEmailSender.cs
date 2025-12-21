using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Infrastructure.Auth;

public sealed class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _logger;

    public LogEmailSender(ILogger<LogEmailSender> logger) => _logger = logger;

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        _logger.LogInformation("EMAIL_SIMULATION To={To} Subject={Subject} Body={Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }


}
