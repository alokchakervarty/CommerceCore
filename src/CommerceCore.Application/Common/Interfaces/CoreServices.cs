using CommerceCore.Contracts.Billing;

namespace CommerceCore.Application.Common.Interfaces;

/// <summary>Read-only access to the authenticated caller, populated by Api middleware
/// from the validated JWT — never set directly by handlers.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsInRole(string role);

    /// <summary>The client-generated anonymous-cart identifier from the X-Guest-Id
    /// header, if the caller sent one. Used to build/hold a cart before login —
    /// see GuestCartMerger for how it's folded into the Customer's cart on login.</summary>
    Guid? GuestId { get; }
}

/// <summary>Resolves which Store the current request belongs to (from a subdomain,
/// custom domain, or an X-Store-Id header depending on Api configuration), so every
/// Application handler can filter/scope by StoreId without knowing how it was resolved.</summary>
public interface ICurrentTenantService
{
    Guid StoreId { get; }
}

public interface IDateTimeService
{
    DateTime UtcNow { get; }
}

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string hash);
}

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, out DateTime expiresAt);
    string GenerateRefreshToken();
}

/// <summary>Sends an email using the target Store's SMTP configuration
/// (StoreSettings.SmtpHost/Port/credentials). Implemented in Infrastructure.</summary>
public interface IEmailSender
{
    Task SendAsync(Guid storeId, string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public interface IEmailApiSender
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

/// <summary>Sends an SMS. The default Infrastructure implementation logs the message
/// rather than calling a real carrier — no SMS provider (Twilio, etc.) is configured
/// out of the box. Swap in a real provider by implementing this interface and
/// re-registering it in Infrastructure's DependencyInjection.</summary>
public interface ISmsSender
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}

/// <summary>Renders a GST tax invoice as a PDF byte array. Implemented in
/// Infrastructure using QuestPDF.</summary>
public interface IInvoicePdfGenerator
{
    byte[] Generate(InvoiceDto invoice);
}
