using System.Security.Claims;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue(ClaimTypes.Name)
                ?? user?.FindFirstValue("sub");
        }
    }

    public bool IsInRole(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.IsInRole(role) == true;
    }
}
