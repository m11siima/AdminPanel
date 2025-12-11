using AdminPanel.Application.Services;
using AdminPanel.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AdminPanel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IPlatformService, PlatformService>();

        return services;
    }
}


