using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data.Entities;
using Server.Api.Services;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var session = await _authService.LoginAsync(request, cancellationToken);
        if (session is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed.",
                Detail = "Invalid username or password.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(new AuthResponse
        {
            AccessToken = session.AccessToken,
            ExpiresAtUtc = session.ExpiresAtUtc,
            User = MapUser(session.User)
        });
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(User, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(MapUser(user));
    }

    private static UserResponse MapUser(UserEntity user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            IsSystemAccount = user.IsSystemAccount,
            CreatedAtUtc = user.CreatedAtUtc,
            UpdatedAtUtc = user.UpdatedAtUtc
        };
    }
}
