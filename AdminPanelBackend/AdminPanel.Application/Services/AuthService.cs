using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private const int RefreshTokenExpirationDays = 30;

    public AuthService(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role!)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null)
        {
            throw new ValidationException("Invalid email or password");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ValidationException("Invalid email or password");
        }

        var isSuperAdmin = user.UserRoles
            .Any(ur => ur.Role != null && ur.Role!.Name == "SuperAdmin");

        var permissionKeys = user.UserRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Key)
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissionKeys, isSuperAdmin);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenString,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays));

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return (accessToken, refreshTokenString);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshTokenString)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User!)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role!)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenString);

        if (refreshToken == null || !refreshToken.IsValid)
        {
            throw new ValidationException("Invalid or expired refresh token");
        }

        var user = refreshToken.User;
        if (user == null)
        {
            throw new EntityNotFoundException("User", refreshToken.UserId);
        }

        refreshToken.Revoke("Used for refresh");

        var isSuperAdmin = user.UserRoles
            .Any(ur => ur.Role != null && ur.Role!.Name == "SuperAdmin");

        var permissionKeys = user.UserRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Key)
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissionKeys, isSuperAdmin);
        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenString,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays));

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return (accessToken, newRefreshTokenString);
    }
}


