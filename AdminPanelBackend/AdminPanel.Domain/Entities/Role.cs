using AdminPanel.Domain.Exceptions;

namespace AdminPanel.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; } = false;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { }

    public static Role Create(string name, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException(nameof(Name), "Role name cannot be empty");

        var normalizedName = name.Trim();
        if (normalizedName.Length > 128)
            throw new ValidationException(nameof(Name), "Role name cannot exceed 128 characters");

        if (description != null && description.Length > 512)
            throw new ValidationException(nameof(Description), "Description cannot exceed 512 characters");

        return new Role
        {
            Name = normalizedName,
            Description = description?.Trim(),
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDescription(string? description)
    {
        if (IsSystem)
            throw new DomainException("Cannot modify system role");

        if (description != null && description.Length > 512)
            throw new ValidationException(nameof(Description), "Description cannot exceed 512 characters");

        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignPermission(Guid permissionId)
    {
        if (RolePermissions.Any(rp => rp.PermissionId == permissionId))
            throw new DomainException($"Role already has permission with id '{permissionId}'");

        RolePermissions.Add(new RolePermission
        {
            RoleId = Id,
            PermissionId = permissionId
        });
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePermission(Guid permissionId)
    {
        if (IsSystem)
            throw new DomainException("Cannot modify permissions for system role");

        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission == null)
            throw new DomainException($"Role does not have permission with id '{permissionId}'");

        RolePermissions.Remove(rolePermission);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPermissions(IEnumerable<Guid> permissionIds)
    {
        if (IsSystem)
            throw new DomainException("Cannot modify permissions for system role");

        var permissionIdsList = permissionIds.ToList();
        var existingPermissionIds = RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

        var toRemove = RolePermissions.Where(rp => !permissionIdsList.Contains(rp.PermissionId)).ToList();
        foreach (var rp in toRemove)
        {
            RolePermissions.Remove(rp);
        }

        foreach (var permissionId in permissionIdsList)
        {
            if (!existingPermissionIds.Contains(permissionId))
            {
                RolePermissions.Add(new RolePermission
                {
                    RoleId = Id,
                    PermissionId = permissionId
                });
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasPermission(Guid permissionId)
    {
        return RolePermissions.Any(rp => rp.PermissionId == permissionId);
    }

    public bool HasPermissionKey(string permissionKey)
    {
        return RolePermissions.Any(rp => rp.Permission != null && rp.Permission.Key == permissionKey);
    }
}

