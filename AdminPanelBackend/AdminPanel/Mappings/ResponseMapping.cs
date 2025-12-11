using System.Linq;
using AdminPanel.Domain.Entities;
using AdminPanel.Responses;

namespace AdminPanel.Mappings;

public static class ResponseMapping
{
    public static UserResponse ToResponse(this User user)
    {
        var roles = user.UserRoles.Select(ur => ur.RoleId).ToArray();
        return new UserResponse(user.Id, user.Email, user.Name, roles);
    }

    public static RoleResponse ToResponse(this Role role)
    {
        var permissionKeys = role.RolePermissions
            .Select(rp => rp.Permission?.Key ?? string.Empty)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToArray();

        return new RoleResponse(role.Id, role.Name, role.Description, role.IsSystem, permissionKeys);
    }

    public static GameResponse ToResponse(this Game game)
    {
        var platformNames = game.GameConfigs
            .Where(gc => gc.Config != null)
            .SelectMany(gc => gc.Config!.Platforms)
            .Select(p => p.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToArray();

        return new GameResponse(
            game.Id,
            game.Title,
            game.Path,
            game.Provider,
            game.Category,
            game.Tags,
            game.IsFeatured,
            game.Display,
            game.CreatedAt,
            game.UpdatedAt,
            platformNames
        );
    }

    public static PlatformResponse ToResponse(this Platform platform)
    {
        return new PlatformResponse(
            platform.Id,
            platform.Name,
            platform.Description,
            platform.Url,
            platform.ConfigId,
            platform.Config?.Name,
            platform.CreatedAt,
            platform.UpdatedAt
        );
    }

    public static ConfigResponse ToResponse(this Config config)
    {
        var gameIds = config.GameConfigs
            .Select(gc => gc.GameId)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var games = config.GameConfigs
            .Select(gc => new Responses.GameConfigItemResponse(gc.GameId, gc.IsEnabled))
            .OrderBy(g => g.GameId)
            .ToArray();

        var platformNames = config.Platforms
            .Select(p => p.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name)
            .ToArray();

        return new ConfigResponse(
            config.Id,
            config.Name,
            config.CreatedAt,
            config.UpdatedAt,
            gameIds,
            games,
            platformNames
        );
    }
}

