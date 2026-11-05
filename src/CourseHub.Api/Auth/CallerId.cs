using System.Security.Claims;

namespace CourseHub.Api.Auth;

public static class CallerId
{
    public static string? From(ClaimsPrincipal user)
    {
        // JwtBearer maps the token's "sub" claim onto NameIdentifier.
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
    }
}
