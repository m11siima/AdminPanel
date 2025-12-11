using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Application.Services;

public class PlatformService : IPlatformService
{
    private readonly IAppDbContext _context;

    public PlatformService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Platform>> GetPlatformsAsync()
    {
        return await _context.Platforms
            .Include(p => p.Config)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Platform?> GetPlatformByIdAsync(Guid platformId)
    {
        return await _context.Platforms
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p => p.Id == platformId);
    }

    public async Task<Platform> CreatePlatformAsync(CreatePlatformRequest request)
    {
        var normalizedName = request.Name.Trim();

        var exists = await _context.Platforms.AnyAsync(p => p.Name == normalizedName);
        if (exists)
        {
            throw new ValidationException(nameof(request.Name), $"Platform with name '{normalizedName}' already exists.");
        }

        var configExists = await _context.Configs.AnyAsync(c => c.Id == request.ConfigId);
        if (!configExists)
        {
            throw new EntityNotFoundException("Config", request.ConfigId);
        }

        var platform = Platform.Create(normalizedName, request.Description, request.Url, request.ConfigId);

        _context.Platforms.Add(platform);
        await _context.SaveChangesAsync();

        return await _context.Platforms
            .Include(p => p.Config)
            .FirstAsync(p => p.Id == platform.Id);
    }

    public async Task<Platform> UpdatePlatformAsync(Guid platformId, UpdatePlatformRequest request)
    {
        var platform = await _context.Platforms
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p => p.Id == platformId);

        if (platform == null)
        {
            throw new EntityNotFoundException("Platform", platformId);
        }

        string? newName = null;
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            newName = request.Name.Trim();
            
            var nameExists = await _context.Platforms
                .AnyAsync(p => p.Name == newName && p.Id != platformId);
            
            if (nameExists)
            {
                throw new ValidationException(nameof(request.Name), $"Platform with name '{newName}' already exists.");
            }
        }

        Guid? newConfigId = null;
        if (request.ConfigId.HasValue)
        {
            var configExists = await _context.Configs.AnyAsync(c => c.Id == request.ConfigId.Value);
            if (!configExists)
            {
                throw new EntityNotFoundException("Config", request.ConfigId.Value);
            }
            newConfigId = request.ConfigId.Value;
        }

        platform.Update(newName, request.Description, request.Url, newConfigId);

        await _context.SaveChangesAsync();

        return await _context.Platforms
            .Include(p => p.Config)
            .FirstAsync(p => p.Id == platform.Id);
    }

    public async Task DeletePlatformAsync(Guid platformId)
    {
        var platform = await _context.Platforms.FindAsync(platformId);

        if (platform == null)
        {
            throw new EntityNotFoundException("Platform", platformId);
        }

        _context.Platforms.Remove(platform);
        await _context.SaveChangesAsync();
    }
}


