using Microsoft.AspNetCore.Authorization;

namespace AdminPanel.Infrastructure.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionKey { get; }

    public PermissionRequirement(string permissionKey)
    {
        PermissionKey = permissionKey ?? throw new ArgumentNullException(nameof(permissionKey));
    }
}

