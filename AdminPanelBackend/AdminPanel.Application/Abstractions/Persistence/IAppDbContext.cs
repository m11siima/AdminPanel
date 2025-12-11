using AdminPanel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    public DbSet<User> Users { get; }
    public DbSet<Role> Roles { get; }
    public DbSet<Permission> Permissions { get; }
    public DbSet<UserRole> UserRoles { get; }
    public DbSet<RolePermission> RolePermissions { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<Game> Games { get; }
    public DbSet<Platform> Platforms { get; }
    public DbSet<Config> Configs { get; }
    public DbSet<GameConfig> GameConfigs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
