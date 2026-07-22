namespace CommerceCore.Domain.Enums;

/// <summary>What an OTP code is being used for. Login doubles as passwordless
/// signup when no matching account exists yet — see OtpCode's doc comment.</summary>
public enum OtpPurpose
{
    Login = 0,
    EmailVerification = 1,
    PhoneVerification = 2,
    PasswordReset = 3
}
