namespace CommerceCore.Contracts.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);

public record AuthResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt);

public record CurrentUserResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    IReadOnlyList<string> Roles,
    DateTime CreatedDate);
