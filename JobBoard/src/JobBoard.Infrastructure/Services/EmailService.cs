using JobBoard.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobBoard.Infrastructure.Services;

// Stub: logs instead of sending. Swap in SendGrid/SES/SMTP here.
// In the API layer, queue this via Hangfire (BackgroundJob.Enqueue<IEmailService>(...))
// so a slow mail provider never blocks an HTTP request.
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Email queued to {ToEmail} - Subject: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
