using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Application.Services;

public class RoleService : IRoleService
{
    private readonly IAppDbContext _context;

    public RoleService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<Role> CreateRoleAsync(CreateRoleRequest request)
    {
        var normalizedName = request.Name.Trim();

        var exists = await _context.Roles.AnyAsync(r => r.Name == normalizedName);
        if (exists)
        {
            throw new ValidationException(nameof(request.Name), $"Role with name '{normalizedName}' already exists.");
        }

        var role = Role.Create(normalizedName, request.Description);

        if (request.PermissionKeys.Count > 0)
        {
            var permissionKeys = request.PermissionKeys.ToList();
            var permissions = await _context.Permissions
                .Where(p => permissionKeys.Contains(p.Key))
                .ToListAsync();

            var foundKeys = permissions.Select(p => p.Key).ToHashSet();
            var missing = permissionKeys.Except(foundKeys).ToList();

            if (missing.Count > 0)
            {
                throw new EntityNotFoundException("Permission", $"One or more permissions not found: {string.Join(", ", missing)}");
            }

            foreach (var permission in permissions)
            {
                role.AssignPermission(permission.Id);
            }
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return role;
    }

    public async Task<Role> SetPermissionsAsync(Guid roleId, UpdateRolePermissionsRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            throw new EntityNotFoundException("Role", roleId);
        }

        var permissionKeys = request.PermissionKeys.ToList();
        var permissions = await _context.Permissions
            .Where(p => permissionKeys.Contains(p.Key))
            .ToListAsync();

        var foundKeys = permissions.Select(p => p.Key).ToHashSet();
        var missing = permissionKeys.Except(foundKeys).ToList();

        if (missing.Count > 0)
        {
            throw new EntityNotFoundException("Permission", $"One or more permissions not found: {string.Join(", ", missing)}");
        }

        var permissionIds = permissions.Select(p => p.Id).ToList();
        role.SetPermissions(permissionIds);

        await _context.SaveChangesAsync();

        return role;
    }
}


