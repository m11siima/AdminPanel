using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IAppDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ToListAsync();
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (exists)
        {
            throw new ValidationException(nameof(request.Email), $"User with email '{normalizedEmail}' already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(normalizedEmail, passwordHash, request.Name);

        if (request.RoleIds.Count > 0)
        {
            var roleIds = request.RoleIds.ToList();
            var existingRoles = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();

            if (existingRoles.Count != roleIds.Count)
            {
                var missing = roleIds.Except(existingRoles).ToList();
                throw new EntityNotFoundException("Role", $"One or more roles not found: {string.Join(", ", missing)}");
            }

            foreach (var roleId in roleIds)
            {
                user.AssignRole(roleId);
            }
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        if (request.Name != null)
        {
            user.UpdateName(request.Name);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordHash = _passwordHasher.Hash(request.Password);
            user.ChangePassword(passwordHash);
        }

        if (request.RoleIds != null)
        {
            var roleIdsList = request.RoleIds.ToList();
            if (roleIdsList.Count > 0)
            {
                var existingRoles = await _context.Roles
                    .Where(r => roleIdsList.Contains(r.Id))
                    .Select(r => r.Id)
                    .ToListAsync();

                if (existingRoles.Count != roleIdsList.Count)
                {
                    var missing = roleIdsList.Except(existingRoles).ToList();
                    throw new EntityNotFoundException("Role", $"One or more roles not found: {string.Join(", ", missing)}");
                }
            }

            user.AssignRoles(roleIdsList);
        }

        await _context.SaveChangesAsync();
        return user;
    }
}


