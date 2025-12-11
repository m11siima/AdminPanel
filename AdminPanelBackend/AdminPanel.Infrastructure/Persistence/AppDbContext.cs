using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<Config> Configs => Set<Config>();
    public DbSet<GameConfig> GameConfigs => Set<GameConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Name).HasMaxLength(256);
            entity.Property(u => u.CreatedAt);
            entity.Property(u => u.UpdatedAt).ValueGeneratedOnUpdate();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(128).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(512);
            entity.Property(r => r.IsSystem).HasDefaultValue(false);
            entity.Property(r => r.CreatedAt);
            entity.Property(r => r.UpdatedAt).ValueGeneratedOnUpdate();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasIndex(p => p.Key).IsUnique();
            entity.Property(p => p.Key).HasMaxLength(128).IsRequired();
            entity.Property(p => p.Resource).HasMaxLength(64).IsRequired();
            entity.Property(p => p.Action).HasMaxLength(32).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(512);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            entity.Property(rt => rt.Token).IsRequired();
            entity.Property(rt => rt.ExpiresAt).IsRequired();
            entity.Property(rt => rt.CreatedAt).IsRequired();
            entity.Property(rt => rt.IsRevoked).HasDefaultValue(false);
            entity.Property(rt => rt.RevokedReason).HasMaxLength(256);

            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Game");
            entity.Metadata.SetIsTableExcludedFromMigrations(true);

            entity.HasMany(g => g.GameConfigs)
                .WithOne(gc => gc.Game)
                .HasForeignKey(gc => gc.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.ToTable("platforms");
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Property(p => p.Name).HasMaxLength(128).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(512);
            entity.Property(p => p.Url).HasMaxLength(512);
            entity.Property(p => p.ConfigId).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.UpdatedAt).ValueGeneratedOnUpdate();

            entity.HasOne(p => p.Config)
                .WithMany(c => c.Platforms)
                .HasForeignKey(p => p.ConfigId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Config>(entity =>
        {
            entity.ToTable("configs");
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Name).HasMaxLength(128).IsRequired();
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.UpdatedAt).ValueGeneratedOnUpdate();

            entity.HasMany(c => c.Platforms)
                .WithOne(p => p.Config)
                .HasForeignKey(p => p.ConfigId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(c => c.GameConfigs)
                .WithOne(gc => gc.Config)
                .HasForeignKey(gc => gc.ConfigId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GameConfig>(entity =>
        {
            entity.ToTable("game_configs");
            entity.HasKey(gc => new { gc.GameId, gc.ConfigId });
            entity.Property(gc => gc.IsEnabled).HasDefaultValue(true);
        });
    }
}

