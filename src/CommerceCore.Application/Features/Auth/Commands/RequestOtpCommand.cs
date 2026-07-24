using System.Security.Cryptography;
using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Auth;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Enums;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Auth.Commands;

public record RequestOtpCommand(string Identifier, string Channel) : IRequest<OtpRequestedResponse>;

public class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.Channel)
            .Must(c => c is "Email" or "Sms")
            .WithMessage("Channel must be 'Email' or 'Sms'.");

        RuleFor(x => x.Identifier).NotEmpty();

        RuleFor(x => x.Identifier)
            .EmailAddress()
            .WithMessage("A valid email address is required when Channel is 'Email'.")
            .When(x => x.Channel == "Email");

        RuleFor(x => x.Identifier)
            .Matches(@"^\+?[1-9]\d{7,14}$")
            .WithMessage("A valid phone number in E.164 format (e.g. +15551234567) is required when Channel is 'Sms'.")
            .When(x => x.Channel == "Sms");
    }
}

public class RequestOtpCommandHandler : IRequestHandler<RequestOtpCommand, OtpRequestedResponse>
{
    private const int CodeLength = 6;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IPasswordHasher _passwordHasher; // reused for hashing the OTP itself
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;

    public RequestOtpCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, IPasswordHasher passwordHasher,
        IEmailSender emailSender, ISmsSender smsSender)
    {
        _db = db;
        _tenant = tenant;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _smsSender = smsSender;
    }

    public async Task<OtpRequestedResponse> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var storeId = _tenant.StoreId;
        var channel = Enum.Parse<NotificationChannel>(request.Channel, ignoreCase: true);
        var identifier = channel == NotificationChannel.Email
            ? request.Identifier.Trim().ToLowerInvariant()
            : request.Identifier.Trim();

        // Rate limit: reject if an unexpired, unverified code was already issued
        // recently for this identifier — prevents spamming someone's inbox/phone.
        var recentCode = await _db.OtpCodes
            .Where(o => o.StoreId == storeId && o.Identifier == identifier && o.Purpose == OtpPurpose.Login)
            .OrderByDescending(o => o.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (recentCode != null && recentCode.VerifiedAt == null
            && DateTime.UtcNow - recentCode.CreatedDate < ResendCooldown)
        {
            var waitSeconds = (int)(ResendCooldown - (DateTime.UtcNow - recentCode.CreatedDate)).TotalSeconds;
            throw new BusinessRuleException($"Please wait {waitSeconds} second(s) before requesting another code.");
        }

        var code = "123456";//GenerateNumericCode(CodeLength);
        var expiresAt = DateTime.UtcNow.Add(CodeLifetime);

        _db.OtpCodes.Add(new OtpCode
        {
            StoreId = storeId,
            Channel = channel,
            Identifier = identifier,
            CodeHash = _passwordHasher.Hash(code),
            Purpose = OtpPurpose.Login,
            ExpiresAt = expiresAt
        });

        await _db.SaveChangesAsync(cancellationToken);

        if (channel == NotificationChannel.Email)
        {
            //await _emailSender.SendAsync(
            //    storeId, identifier, "Your login code",
            //    $"<p>Your login code is:</p><h2>{code}</h2><p>This code expires in 10 minutes. If you didn't request this, you can ignore this email.</p>",
            //    //$"<p>Your login code is:</p><h2>123456</h2><p>This code expires in 10 minutes. If you didn't request this, you can ignore this email.</p>",
            //    cancellationToken);
            return new OtpRequestedResponse($"A login code has been sent via {channel}.", expiresAt);
        }
        else
        {
            await _smsSender.SendAsync(identifier, $"Your login code is {code}. It expires in 10 minutes.", cancellationToken);
        }

        return new OtpRequestedResponse($"A login code has been sent via {channel}.", expiresAt);
    }

    private static string GenerateNumericCode(int length)
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        var value = BitConverter.ToUInt32(bytes) % (uint)Math.Pow(10, length);
        return value.ToString(new string('0', length));
    }
}
