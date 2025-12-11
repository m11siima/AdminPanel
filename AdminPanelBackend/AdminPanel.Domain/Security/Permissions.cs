namespace AdminPanel.Domain.Security;

/// <summary>
/// All permission keys must be defined here as constants to avoid typos and enable refactoring.
/// </summary>
public static class Permissions
{
    // Example: Game Management permissions (for future implementation)
    // This is just an example structure - actual Game Management module is not implemented yet
    public static class GM
    {
        public static class Module
        {
            public const string Access = "gm.module.access";
        }

        public static class Games
        {
            public const string Read = "gm.games.read";
            public const string Edit = "gm.games.edit";
            public const string Feature = "gm.games.feature.set";
            public const string Visibility = "gm.games.visibility.update";

            public static class Platform
            {
                public const string Toggle = "gm.games.platform.toggle";

                public static string ToggleFor(string platform) => $"gm.games.platform.toggle:{platform}";
            }
        }

        public static class Presets
        {
            public const string Manage = "gm.presets.manage";
        }

        public static class Config
        {
            public const string Read = "gm.config.read";
            public const string Create = "gm.config.create";
            public const string Update = "gm.config.update";
            public const string Delete = "gm.config.delete";
            public const string Import = "gm.config.import";
            public const string Export = "gm.config.export";
        }

        public static class Platforms
        {
            public const string Read = "gm.platforms.read";
            public const string Create = "gm.platforms.create";
            public const string Update = "gm.platforms.update";
            public const string Delete = "gm.platforms.delete";
        }
    }

    // User Management permissions
    public static class Users
    {
        public const string Read = "users.read";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string Manage = "users.manage";
    }

    // Role Management permissions
    public static class Roles
    {
        public const string Read = "roles.read";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string Manage = "roles.manage";
    }

    /// <summary>
    /// Collection of all permission keys for seeding and validation.
    /// </summary>
    public static readonly IReadOnlyCollection<string> All = new[]
    {
        // Game Management
        GM.Module.Access,
        GM.Games.Read,
        GM.Games.Edit,
        GM.Games.Feature,
        GM.Games.Visibility,
        GM.Games.Platform.Toggle,
        GM.Presets.Manage,
        GM.Config.Read,
        GM.Config.Create,
        GM.Config.Update,
        GM.Config.Delete,
        GM.Config.Import,
        GM.Config.Export,
        GM.Platforms.Read,
        GM.Platforms.Create,
        GM.Platforms.Update,
        GM.Platforms.Delete,

        // User Management
        Users.Read,
        Users.Create,
        Users.Update,
        Users.Delete,
        Users.Manage,

        // Role Management
        Roles.Read,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Roles.Manage,
    };
}

