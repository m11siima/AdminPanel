using AdminPanel.Domain.Exceptions;

namespace AdminPanel.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { }

    public static User Create(string email, string passwordHash, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException(nameof(Email), "Email cannot be empty");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ValidationException(nameof(PasswordHash), "Password hash cannot be empty");

        if (!IsValidEmail(email))
            throw new ValidationException(nameof(Email), "Invalid email format");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ValidationException(nameof(PasswordHash), "Password hash cannot be empty");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string? name)
    {
        Name = name?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRole(Guid roleId)
    {
        if (UserRoles.Any(ur => ur.RoleId == roleId))
            throw new DomainException($"User already has role with id '{roleId}'");

        UserRoles.Add(new UserRole
        {
            UserId = Id,
            RoleId = roleId
        });
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
            throw new DomainException($"User does not have role with id '{roleId}'");

        UserRoles.Remove(userRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRoles(IEnumerable<Guid> roleIds)
    {
        var roleIdsList = roleIds.ToList();
        var existingRoleIds = UserRoles.Select(ur => ur.RoleId).ToHashSet();

        var toRemove = UserRoles.Where(ur => !roleIdsList.Contains(ur.RoleId)).ToList();
        foreach (var ur in toRemove)
        {
            UserRoles.Remove(ur);
        }

        foreach (var roleId in roleIdsList)
        {
            if (!existingRoleIds.Contains(roleId))
            {
                UserRoles.Add(new UserRole
                {
                    UserId = Id,
                    RoleId = roleId
                });
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasRole(Guid roleId)
    {
        return UserRoles.Any(ur => ur.RoleId == roleId);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

