using Microsoft.AspNetCore.Authorization;

namespace ProfileService.Authorization
{
    public class RequireUserIdRequirement : IAuthorizationRequirement
    {
    }
}
