using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Identity;

/// <summary>
/// A one-time passcode issued for login, email/phone verification, or password
/// reset. Identifier is the raw email address or phone number the code was sent to —
/// looked up directly rather than requiring a User to already exist, since OTP Login
/// doubles as passwordless signup: a request against an unrecognized identifier still
/// gets a code, and successful verification creates the account on the spot.
/// The code itself is never stored in plaintext — only CodeHash — same principle as
/// a password.
/// </summary>
public class OtpCode : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public NotificationChannel Channel { get; set; } // Email or Sms
    public string Identifier { get; set; } = string.Empty; // email address or E.164 phone number
    public string CodeHash { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }

    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public const int MaxAttempts = 5;

    public DateTime? VerifiedAt { get; set; }

    /// <summary>Set once the code is verified and resolved to a User (existing or
    /// newly created via passwordless signup).</summary>
    public Guid? UserId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsVerified => VerifiedAt != null;
    public bool IsUsable => !IsExpired && !IsVerified && AttemptCount < MaxAttempts;
}
