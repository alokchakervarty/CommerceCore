using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Auth;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _db = db;
        _tenant = tenant;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var storeId = _tenant.StoreId;
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.StoreId == storeId && u.Email == emailNormalized, cancellationToken);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenAppException("This account has been deactivated.");

        if (user.IsLockedOut && user.LockoutEndDate > DateTime.UtcNow)
            throw new ForbiddenAppException("This account is temporarily locked. Please try again later.");

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

        user.LastLoginDate = DateTime.UtcNow;
        user.AccessFailedCount = 0;

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, roleNames, accessToken, refreshToken, expiresAt);
    }
}
