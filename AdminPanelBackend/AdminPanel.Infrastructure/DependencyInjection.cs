using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Infrastructure.Persistence;
using AdminPanel.Infrastructure.Security;
using AdminPanel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdminPanel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                              ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        var versionString = configuration["Database:ServerVersion"];
        var serverVersion = string.IsNullOrWhiteSpace(versionString)
            ? new MySqlServerVersion(new Version(8, 0, 36))
            : new MySqlServerVersion(Version.Parse(versionString));

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IPermissionCatalogSeeder, PermissionCatalogSeeder>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}

