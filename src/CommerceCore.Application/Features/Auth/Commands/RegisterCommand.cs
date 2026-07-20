using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Auth;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Auth.Commands;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
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

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var storeId = _tenant.StoreId;
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.StoreId == storeId && u.Email == emailNormalized, cancellationToken);
        if (exists)
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            StoreId = storeId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = emailNormalized,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailConfirmed = false,
            IsActive = true
        };

        _db.Users.Add(user);

        // Every new registrant gets the store's default "Customer" role, if one exists.
        var customerRole = await _db.Roles.FirstOrDefaultAsync(
            r => r.StoreId == storeId && r.Name == "Customer", cancellationToken);

        if (customerRole != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = customerRole.Id });
        }

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Email, customerRole != null ? new[] { customerRole.Name } : Array.Empty<string>(), out var expiresAt);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            customerRole != null ? new[] { customerRole.Name } : Array.Empty<string>(),
            accessToken,
            refreshToken,
            expiresAt);
    }
}
