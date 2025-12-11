using Microsoft.AspNetCore.Authorization;

namespace AdminPanel.Infrastructure.Security;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        PermissionRequirement requirement)
    {
        if (authContext.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (authContext.User.HasClaim("is_superadmin", "true"))
        {
            authContext.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasPermission = authContext.User
            .FindAll("permission")
            .Any(c => c.Value == requirement.PermissionKey);

        if (hasPermission)
        {
            authContext.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

