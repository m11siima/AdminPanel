namespace AdminPanel.Domain.Security;

/// <summary>
/// Logical groups of permissions for easier role management in UI.
/// </summary>
public static class PermissionGroups
{
    /// <summary>
    /// All Game Management permissions.
    /// </summary>
    public static readonly string[] GameManagement =
    {
        Permissions.GM.Module.Access,
        Permissions.GM.Games.Read,
        Permissions.GM.Games.Edit,
        Permissions.GM.Games.Feature,
        Permissions.GM.Games.Visibility,
        Permissions.GM.Games.Platform.Toggle,
        Permissions.GM.Presets.Manage,
        Permissions.GM.Config.Read,
        Permissions.GM.Config.Create,
        Permissions.GM.Config.Update,
        Permissions.GM.Config.Delete,
        Permissions.GM.Config.Import,
        Permissions.GM.Config.Export,
        Permissions.GM.Platforms.Read,
        Permissions.GM.Platforms.Create,
        Permissions.GM.Platforms.Update,
        Permissions.GM.Platforms.Delete,
    };

    /// <summary>
    /// All User Management permissions.
    /// </summary>
    public static readonly string[] UserManagement =
    {
        Permissions.Users.Read,
        Permissions.Users.Create,
        Permissions.Users.Update,
        Permissions.Users.Delete,
        Permissions.Users.Manage,
    };

    /// <summary>
    /// All Role Management permissions.
    /// </summary>
    public static readonly string[] RoleManagement =
    {
        Permissions.Roles.Read,
        Permissions.Roles.Create,
        Permissions.Roles.Update,
        Permissions.Roles.Delete,
        Permissions.Roles.Manage,
    };

    /// <summary>
    /// Full admin access - all permissions.
    /// </summary>
    public static readonly string[] FullAdmin = Permissions.All.ToArray();
}

