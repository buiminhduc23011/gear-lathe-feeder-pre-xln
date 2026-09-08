using System.Security.Claims;
using Server.Api.Contracts.Requests;
using Server.Api.Data.Entities;

namespace Server.Api.Services;

public interface IAuthService
{
    Task<AuthenticatedSession?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserEntity?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
