using System.Security.Claims;
using Application.Exceptions;
using Application.Features.Identity.Users;

namespace Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private ClaimsPrincipal _principal = null!;

    public string Name => _principal.Identity?.Name! ?? throw new ConflictException(["ClaimsPrincipal not set"]);

    public IEnumerable<Claim> GetUserClaims()
    {
        if (_principal is null) throw new ConflictException(["ClaimsPrincipal not set"]);
        return _principal.Claims;
    }

    public string GetUserEmail()
    {
        if (_principal is null) throw new ConflictException(["ClaimsPrincipal not set"]);
        if (IsAuthenticated())
        {
            return _principal.GetEmail()!;
        }
        return string.Empty;
    }

    public string GetUserId()
    {
        if (_principal is null) throw new ConflictException(["ClaimsPrincipal not set"]);
        if (IsAuthenticated())
        {
            return _principal.GetUserId()!;
        }
        return string.Empty;
    }

    public string GetUserTenant()
    {
        if (_principal is null) throw new ConflictException(["ClaimsPrincipal not set"]);
        if (IsAuthenticated())
        {
            return _principal.GetTenant()!;
        }
        return string.Empty;
    }

    public bool IsAuthenticated()
    {
        if (_principal is null) throw new ConflictException(["ClaimsPrincipal not set"]);
        return _principal.Identity!.IsAuthenticated;
    }

    public bool IsInRole(string roleName)
    {
        if (_principal is null) throw new ConflictException(["ClaimsPrincipal not set"]);
        return _principal.IsInRole(roleName);
    }

    public void SetCurrentUser(ClaimsPrincipal principal)
    {
        if (_principal is not null) throw new ConflictException(["Invalid operation on claim"]);
        _principal = principal;
    }
}
