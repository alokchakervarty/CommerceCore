using CommerceCore.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommerceCore.Infrastructure.Services;

/// <summary>
/// Default ISmsSender implementation: logs the message rather than sending a real
/// SMS. No carrier (Twilio, AWS SNS, MSG91, etc.) is wired up in this build, since
/// that requires an account and API keys specific to your deployment.
///
/// To go live: implement ISmsSender against your chosen provider's SDK and swap the
/// registration in Infrastructure/DependencyInjection.cs:
///   services.AddScoped&lt;ISmsSender, YourProviderSmsSender&gt;();
/// Everything upstream (the OTP request/login handlers) is already provider-agnostic
/// and needs no changes.
/// </summary>
public class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "SMS SENDER NOT CONFIGURED — would send to {PhoneNumber}: {Message}. " +
            "Implement ISmsSender against a real provider before relying on SMS OTP in production.",
            toPhoneNumber, message);
        return Task.CompletedTask;
    }
}
