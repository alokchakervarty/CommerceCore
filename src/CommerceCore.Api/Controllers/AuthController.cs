using Asp.Versioning;
using CommerceCore.Application.Features.Auth.Commands;
using CommerceCore.Application.Features.Auth.Queries;
using CommerceCore.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceCore.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password, request.PhoneNumber),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
        return Ok(result);
    }

    [HttpPost("otp/request")]
    [ProducesResponseType(typeof(OtpRequestedResponse), 200)]
    public async Task<ActionResult<OtpRequestedResponse>> RequestOtp(RequestOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RequestOtpCommand(request.Identifier, request.Channel), cancellationToken);
        return Ok(result);
    }

    [HttpPost("otp/login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<ActionResult<AuthResponse>> LoginWithOtp(LoginWithOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginWithOtpCommand(request.Identifier, request.Channel, request.Code), cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), 200)]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(result);
    }
}
