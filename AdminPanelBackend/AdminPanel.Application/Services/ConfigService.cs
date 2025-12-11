using System.Text;
using System.Text.Json;
using AdminPanel.Application.Abstractions.Persistence;
using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Application.Services;

public class ConfigService : IConfigService
{
    private readonly IAppDbContext _context;

    public ConfigService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Config>> GetConfigsAsync()
    {
        return await _context.Configs
            .Include(c => c.Platforms)
            .Include(c => c.GameConfigs)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Config?> GetConfigByIdAsync(Guid configId)
    {
        return await _context.Configs
            .Include(c => c.Platforms)
            .Include(c => c.GameConfigs)
            .FirstOrDefaultAsync(c => c.Id == configId);
    }

    public async Task<Config> CreateConfigAsync(CreateConfigRequest request)
    {
        var normalizedName = request.Name.Trim();

        var exists = await _context.Configs.AnyAsync(c => c.Name == normalizedName);
        if (exists)
        {
            throw new ValidationException(nameof(request.Name), $"Config with name '{normalizedName}' already exists.");
        }

        var config = Config.Create(normalizedName);

        if (request.GameIds.Count > 0)
        {
            var gameIds = request.GameIds.Distinct().ToList();
            var existingGames = await _context.Games
                .Where(g => gameIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            if (existingGames.Count != gameIds.Count)
            {
                var missing = gameIds.Except(existingGames).ToList();
                throw new EntityNotFoundException("Game", $"One or more games not found: {string.Join(", ", missing)}");
            }

            config.AssignGames(gameIds);
        }

        _context.Configs.Add(config);
        await _context.SaveChangesAsync();

        return config;
    }

    public async Task<Config> UpdateConfigAsync(Guid configId, UpdateConfigRequest request)
    {
        var config = await _context.Configs
            .Include(c => c.Platforms)
            .Include(c => c.GameConfigs)
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (config == null)
        {
            throw new EntityNotFoundException("Config", configId);
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var normalizedName = request.Name.Trim();
            
            var nameExists = await _context.Configs
                .AnyAsync(c => c.Name == normalizedName && c.Id != configId);
            
            if (nameExists)
            {
                throw new ValidationException(nameof(request.Name), $"Config with name '{normalizedName}' already exists.");
            }

            config.UpdateName(normalizedName);
        }

        if (request.Games != null)
        {
            var gamesList = request.Games.DistinctBy(g => g.GameId).ToList();
            
            if (gamesList.Count > 0)
            {
                var gameIds = gamesList.Select(g => g.GameId).ToList();
                var existingGames = await _context.Games
                    .Where(g => gameIds.Contains(g.Id))
                    .Select(g => g.Id)
                    .ToListAsync();

                if (existingGames.Count != gameIds.Count)
                {
                    var missing = gameIds.Except(existingGames).ToList();
                    throw new EntityNotFoundException("Game", $"One or more games not found: {string.Join(", ", missing)}");
                }
            }

            config.UpdateGames(gamesList);
        }

        await _context.SaveChangesAsync();
        return config;
    }

    public async Task DeleteConfigAsync(Guid configId)
    {
        var config = await _context.Configs
            .Include(c => c.Platforms)
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (config == null)
        {
            throw new EntityNotFoundException("Config", configId);
        }

        if (config.HasPlatforms())
        {
            throw new DomainException("Cannot delete config that is associated with one or more platforms.");
        }

        _context.Configs.Remove(config);
        await _context.SaveChangesAsync();
    }

    public async Task<ConfigExportDto> ExportConfigAsync(Guid configId)
    {
        var config = await _context.Configs
            .Include(c => c.GameConfigs)
            .FirstOrDefaultAsync(c => c.Id == configId);

        if (config == null)
        {
            throw new EntityNotFoundException("Config", configId);
        }

        var games = config.GameConfigs
            .Select(gc => new GameConfigItemDto
            {
                GameId = gc.GameId,
                IsEnabled = gc.IsEnabled
            })
            .OrderBy(g => g.GameId)
            .ToList();

        return new ConfigExportDto
        {
            Name = config.Name,
            Games = games
        };
    }

    public async Task<Config> ImportConfigAsync(ImportConfigRequest request)
    {
        var normalizedName = request.Name.Trim();

        var exists = await _context.Configs.AnyAsync(c => c.Name == normalizedName);
        if (exists)
        {
            throw new ValidationException(nameof(request.Name), $"Config with name '{normalizedName}' already exists.");
        }

        var config = Config.Create(normalizedName);

        if (request.Games != null && request.Games.Count > 0)
        {
            var gamesList = request.Games.DistinctBy(g => g.GameId).ToList();
            var gameIds = gamesList.Select(g => g.GameId).ToList();

            var existingGames = await _context.Games
                .Where(g => gameIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            if (existingGames.Count != gameIds.Count)
            {
                var missing = gameIds.Except(existingGames).ToList();
                throw new EntityNotFoundException("Game", $"One or more games not found: {string.Join(", ", missing)}");
            }

            var gameConfigItems = gamesList.Select(g => new GameConfigItem
            {
                GameId = g.GameId,
                IsEnabled = g.IsEnabled
            }).ToList();

            config.UpdateGames(gameConfigItems);
        }

        _context.Configs.Add(config);
        await _context.SaveChangesAsync();

        return config;
    }

    public async Task<ConfigFileExportResult> ExportConfigFileAsync(Guid configId)
    {
        var exportDto = await ExportConfigAsync(configId);

        var json = JsonSerializer.Serialize(exportDto, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var fileName = $"{exportDto.Name.Replace(" ", "_")}_config.json";
        var bytes = Encoding.UTF8.GetBytes(json);

        return new ConfigFileExportResult
        {
            Content = bytes,
            FileName = fileName,
            ContentType = "application/json"
        };
    }

    public async Task<Config> ImportConfigFileAsync(Stream fileStream, string fileName)
    {
        if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(nameof(fileName), "Only JSON files are supported.");
        }

        using var reader = new StreamReader(fileStream, Encoding.UTF8, leaveOpen: true);
        var jsonContent = await reader.ReadToEndAsync();

        ImportConfigRequest? importRequest;
        try
        {
            importRequest = JsonSerializer.Deserialize<ImportConfigRequest>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            throw new ValidationException(nameof(fileName), $"Invalid JSON format: {ex.Message}");
        }

        if (importRequest == null || string.IsNullOrWhiteSpace(importRequest.Name))
        {
            throw new ValidationException(nameof(fileName), "Config name is required.");
        }

        return await ImportConfigAsync(importRequest);
    }
}

