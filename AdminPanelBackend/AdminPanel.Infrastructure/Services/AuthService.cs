using System;
using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private const int RefreshTokenExpirationDays = 30;

    public AuthService(
        AppDbContext context,
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

        // Check if user is SuperAdmin
        var isSuperAdmin = user.UserRoles
            .Any(ur => ur.Role != null && ur.Role!.Name == "SuperAdmin");

        // Collect all permissions from user's roles
        var permissionKeys = user.UserRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Key)
            .Distinct()
            .ToList();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissionKeys, isSuperAdmin);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        // Save refresh token to database
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

        // Revoke old refresh token
        refreshToken.Revoke("Used for refresh");

        // Check if user is SuperAdmin
        var isSuperAdmin = user.UserRoles
            .Any(ur => ur.Role != null && ur.Role!.Name == "SuperAdmin");

        // Collect all permissions from user's roles
        var permissionKeys = user.UserRoles
            .Where(ur => ur.Role != null)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Key)
            .Distinct()
            .ToList();

        // Generate new tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user, permissionKeys, isSuperAdmin);
        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();

        // Save new refresh token
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenString,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays));

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return (accessToken, newRefreshTokenString);
    }
}

