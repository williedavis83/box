using System.Security.Claims;
using BoxBottom.Auth.Contract;
using Microsoft.AspNetCore.Http;

namespace BoxBottom.Auth.Business;

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(AuthClaimTypes.BoxUserId);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true
        && UserId.HasValue;
}
