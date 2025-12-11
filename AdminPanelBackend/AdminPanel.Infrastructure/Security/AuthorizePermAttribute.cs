using Microsoft.AspNetCore.Authorization;

namespace AdminPanel.Infrastructure.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizePermAttribute : AuthorizeAttribute
{
    public AuthorizePermAttribute(string permissionKey)
    {
        Policy = $"perm:{permissionKey}";
    }
}

