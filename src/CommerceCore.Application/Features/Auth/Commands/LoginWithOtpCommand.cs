using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Auth;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Auth.Commands;

public record LoginWithOtpCommand(string Identifier, string Channel, string Code) : IRequest<AuthResponse>;

public class LoginWithOtpCommandValidator : AbstractValidator<LoginWithOtpCommand>
{
    public LoginWithOtpCommandValidator()
    {
        RuleFor(x => x.Channel).Must(c => c is "Email" or "Sms").WithMessage("Channel must be 'Email' or 'Sms'.");
        RuleFor(x => x.Identifier).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$").WithMessage("Code must be 6 digits.");
    }
}

public class LoginWithOtpCommandHandler : IRequestHandler<LoginWithOtpCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginWithOtpCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _db = db;
        _tenant = tenant;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginWithOtpCommand request, CancellationToken cancellationToken)
    {
        var storeId = _tenant.StoreId;
        var channel = Enum.Parse<NotificationChannel>(request.Channel, ignoreCase: true);
        var identifier = channel == NotificationChannel.Email
            ? request.Identifier.Trim().ToLowerInvariant()
            : request.Identifier.Trim();

        var otp = await _db.OtpCodes
            .Where(o => o.StoreId == storeId && o.Identifier == identifier
                && o.Channel == channel && o.Purpose == OtpPurpose.Login)
            .OrderByDescending(o => o.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp == null)
            throw new BusinessRuleException("No login code was requested for this identifier. Request a new code first.");

        if (otp.IsVerified)
            throw new BusinessRuleException("This code has already been used. Request a new code.");

        if (otp.IsExpired)
            throw new BusinessRuleException("This code has expired. Request a new code.");

        if (otp.AttemptCount >= OtpCode.MaxAttempts)
            throw new BusinessRuleException("Too many incorrect attempts. Request a new code.");

        if (!_passwordHasher.Verify(request.Code, otp.CodeHash))
        {
            otp.AttemptCount += 1;
            await _db.SaveChangesAsync(cancellationToken);
            throw new BusinessRuleException($"Incorrect code. {OtpCode.MaxAttempts - otp.AttemptCount} attempt(s) remaining.");
        }

        // Code is correct — find the matching account, or create one on the spot
        // (passwordless signup). To require an existing account instead, replace the
        // "?? CreateUser(...)" call below with a NotFoundException throw.
        var user = channel == NotificationChannel.Email
            ? await _db.Users.FirstOrDefaultAsync(u => u.StoreId == storeId && u.Email == identifier, cancellationToken)
            : await _db.Users.FirstOrDefaultAsync(u => u.StoreId == storeId && u.PhoneNumber == identifier, cancellationToken);

        if (user == null)
        {
            user = new User
            {
                StoreId = storeId,
                FirstName = "New",
                LastName = "User",
                Email = channel == NotificationChannel.Email ? identifier : $"{Guid.NewGuid()}@placeholder.local",
                PhoneNumber = channel == NotificationChannel.Sms ? identifier : null,
                EmailConfirmed = channel == NotificationChannel.Email,
                PhoneNumberConfirmed = channel == NotificationChannel.Sms,
                // No password set — this account can only log in via OTP until the
                // user sets one through a future "set password" flow.
                PasswordHash = _passwordHasher.Hash(Guid.NewGuid().ToString()),
                IsActive = true
            };
            _db.Users.Add(user);

            var customerRole = await _db.Roles.FirstOrDefaultAsync(r => r.StoreId == storeId && r.Name == "Customer", cancellationToken);
            if (customerRole != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = customerRole.Id });
            }
        }
        else if (channel == NotificationChannel.Email)
        {
            user.EmailConfirmed = true; // verifying via email OTP proves ownership
        }
        else
        {
            user.PhoneNumberConfirmed = true;
        }

        otp.VerifiedAt = DateTime.UtcNow;
        otp.UserId = user.Id;
        user.LastLoginDate = DateTime.UtcNow;

        var roleNames = await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name
        ).ToListAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roleNames, out var expiresAt);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, roleNames, accessToken, refreshToken, expiresAt);
    }
}
