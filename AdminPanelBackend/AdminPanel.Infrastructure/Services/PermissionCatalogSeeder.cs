using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Security;
using AdminPanel.Domain.Entities;
using AdminPanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AdminPanel.Infrastructure.Services;

public class PermissionCatalogSeeder : IPermissionCatalogSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    private const string SuperAdminRoleName = "SuperAdmin";
    private const string GameManagerRoleName = "GameManager";
    private const string GameManagerEmail = "examplemanager@example.com";
    private const string GameManagerPassword = "pass123";

    public PermissionCatalogSeeder(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        var existingKeys = await _context.Permissions
            .Select(p => p.Key)
            .ToListAsync(cancellationToken);

        var existingKeysSet = existingKeys.ToHashSet();
        var allKeys = Permissions.All.ToHashSet();

        // Find missing permissions
        var toAdd = allKeys
            .Where(key => !existingKeysSet.Contains(key))
            .ToList();

        if (toAdd.Count > 0)
        {
            var newPermissions = toAdd.Select(key =>
            {
                var parts = key.Split('.', 2);
                var resource = parts.Length > 0 ? parts[0] : key;
                var action = parts.Length > 1 ? parts[1] : "unknown";

                return new Permission
                {
                    Key = key,
                    Resource = resource,
                    Action = action,
                    Description = $"Permission: {key}"
                };
            }).ToList();

            _context.Permissions.AddRange(newPermissions);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await SeedSuperAdminAsync(cancellationToken);
        await SeedGameManagerAsync(cancellationToken);
    }

    private async Task SeedSuperAdminAsync(CancellationToken cancellationToken = default)
    {
        var superAdminRole = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == SuperAdminRoleName, cancellationToken);

        if (superAdminRole == null)
        {
            superAdminRole = Role.Create(
                SuperAdminRoleName,
                "Super Administrator with full access to all features",
                isSystem: true);

            _context.Roles.Add(superAdminRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var allPermissions = await _context.Permissions.ToListAsync(cancellationToken);
        var existingPermissionIds = superAdminRole.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var permissionsToAdd = allPermissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .ToList();

        if (permissionsToAdd.Count > 0)
        {
            foreach (var permission in permissionsToAdd)
            {
                superAdminRole.AssignPermission(permission.Id);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        var superAdminEmail = _configuration["SuperAdmin:Email"] ?? "admin@example.com";
        var superAdminPassword = _configuration["SuperAdmin:Password"] ?? "Admin123!";
        var superAdminName = _configuration["SuperAdmin:Name"] ?? "Super Administrator";

        var normalizedEmail = superAdminEmail.Trim().ToLowerInvariant();
        var superAdminUser = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (superAdminUser == null)
        {
            var passwordHash = _passwordHasher.Hash(superAdminPassword);
            superAdminUser = User.Create(normalizedEmail, passwordHash, superAdminName);
            superAdminUser.AssignRole(superAdminRole.Id);

            _context.Users.Add(superAdminUser);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (!superAdminUser.UserRoles.Any(ur => ur.RoleId == superAdminRole.Id))
            {
                superAdminUser.AssignRole(superAdminRole.Id);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task SeedGameManagerAsync(CancellationToken cancellationToken = default)
    {
        var gmRole = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == GameManagerRoleName, cancellationToken);

        if (gmRole == null)
        {
            gmRole = Role.Create(
                GameManagerRoleName,
                "Game Management role",
                isSystem: false);

            _context.Roles.Add(gmRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var gmPermissionKeys = PermissionGroups.GameManagement.ToHashSet();
        var permissions = await _context.Permissions
            .Where(p => gmPermissionKeys.Contains(p.Key))
            .ToListAsync(cancellationToken);

        var existingPermissionIds = gmRole.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var toAdd = permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .ToList();

        if (toAdd.Count > 0)
        {
            foreach (var permission in toAdd)
            {
                gmRole.AssignPermission(permission.Id);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        var normalizedEmail = GameManagerEmail.Trim().ToLowerInvariant();
        var gmUser = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (gmUser == null)
        {
            var passwordHash = _passwordHasher.Hash(GameManagerPassword);
            gmUser = User.Create(normalizedEmail, passwordHash, "Game Manager");
            gmUser.AssignRole(gmRole.Id);

            _context.Users.Add(gmUser);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (!gmUser.UserRoles.Any(ur => ur.RoleId == gmRole.Id))
            {
                gmUser.AssignRole(gmRole.Id);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

