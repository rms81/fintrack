using System.Security.Claims;
using FinTrack.Core.Services;

namespace FinTrack.Host.Auth;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? Id => User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub");

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? DisplayName => User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("name");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
