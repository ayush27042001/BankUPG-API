using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BankUPG.API.Authorization
{
    /// <summary>
    /// Allows a user with the SuperAdmin role to bypass every authorization requirement,
    /// preventing any 401 / 403 response solely due to missing role / policy checks.
    /// </summary>
    public class SuperAdminAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(ClaimTypes.Role, "SuperAdmin"))
            {
                foreach (var requirement in context.PendingRequirements.ToList())
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
