using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Exceptions;
using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Services;

public class GameService : IGameService
{
    private readonly AppDbContext _context;

    public GameService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Game> Games, int TotalCount)> GetGamesAsync(GameListQuery query)
    {
        var gamesQuery = _context.Games
            .Include(g => g.GameConfigs)
                .ThenInclude(gc => gc.Config!)
                    .ThenInclude(c => c.Platforms)
            .AsQueryable();

        // Search filters
        if (!string.IsNullOrWhiteSpace(query.SearchTitle))
        {
            var searchTerm = query.SearchTitle.Trim().ToLowerInvariant();
            gamesQuery = gamesQuery.Where(g =>
                g.Title != null && g.Title.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchPath))
        {
            var searchTerm = query.SearchPath.Trim().ToLowerInvariant();
            gamesQuery = gamesQuery.Where(g =>
                g.Path != null && g.Path.ToLower().Contains(searchTerm));
        }

        // Backward compatibility: if old Search parameter is used, search in both title and path
        if (!string.IsNullOrWhiteSpace(query.Search) && 
            string.IsNullOrWhiteSpace(query.SearchTitle) && 
            string.IsNullOrWhiteSpace(query.SearchPath))
        {
            var searchTerm = query.Search.Trim().ToLowerInvariant();
            gamesQuery = gamesQuery.Where(g =>
                (g.Title != null && g.Title.ToLower().Contains(searchTerm)) ||
                (g.Path != null && g.Path.ToLower().Contains(searchTerm)));
        }

        // Provider filter
        if (!string.IsNullOrWhiteSpace(query.Provider))
        {
            gamesQuery = gamesQuery.Where(g => g.Provider == query.Provider);
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            gamesQuery = gamesQuery.Where(g => g.Category == query.Category);
        }

        // Sorting
        var sortBy = query.SortBy?.ToLowerInvariant() ?? "id";
        var sortOrder = query.SortOrder?.ToLowerInvariant() ?? "asc";

        gamesQuery = sortBy switch
        {
            "provider" => sortOrder == "desc"
                ? gamesQuery.OrderByDescending(g => g.Provider)
                : gamesQuery.OrderBy(g => g.Provider),
            "category" => sortOrder == "desc"
                ? gamesQuery.OrderByDescending(g => g.Category)
                : gamesQuery.OrderBy(g => g.Category),
            "type" => sortOrder == "desc"
                ? gamesQuery.OrderByDescending(g => g.Category)
                : gamesQuery.OrderBy(g => g.Category),
            _ => sortOrder == "desc"
                ? gamesQuery.OrderByDescending(g => g.Id)
                : gamesQuery.OrderBy(g => g.Id)
        };

        // Get total count before pagination
        var totalCount = await gamesQuery.CountAsync();

        // Pagination
        var games = await gamesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (games, totalCount);
    }

    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        return await _context.Games
            .Include(g => g.GameConfigs)
                .ThenInclude(gc => gc.Config!)
                    .ThenInclude(c => c.Platforms)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game> UpdateGameAsync(int gameId, UpdateGameRequest request)
    {
        var game = await _context.Games
            .Include(g => g.GameConfigs)
                .ThenInclude(gc => gc.Config!)
                    .ThenInclude(c => c.Platforms)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new EntityNotFoundException("Game", gameId);
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            game.Title = request.Title;
        }

        if (!string.IsNullOrEmpty(request.Path))
        {
            var newPath = request.Path.Trim();
            // Ensure path is unique, append suffixes if needed
            game.Path = await EnsureUniquePathAsync(newPath, gameId);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            game.Category = request.Category;
        }

        if (!string.IsNullOrEmpty(request.Tags))
        {
            game.Tags = request.Tags;
        }

        game.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return game;
    }

    public async Task<Game> SetGameFeaturedAsync(int gameId, SetGameFeaturedRequest request)
    {
        var game = await _context.Games
            .Include(g => g.GameConfigs)
                .ThenInclude(gc => gc.Config!)
                    .ThenInclude(c => c.Platforms)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            throw new EntityNotFoundException("Game", gameId);
        }

        game.IsFeatured = request.IsFeatured;
        game.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return game;
    }

    public async Task ToggleGamePlatformAsync(int gameId, string platform, bool enabled)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null)
        {
            throw new EntityNotFoundException("Game", gameId);
        }

        // Find platform by name
        var platformEntity = await _context.Platforms
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p => p.Name == platform);

        if (platformEntity == null)
        {
            throw new EntityNotFoundException("Platform", platform);
        }

        var configId = platformEntity.ConfigId;

        // Find or create GameConfig relationship
        var gameConfig = await _context.GameConfigs
            .FirstOrDefaultAsync(gc => gc.GameId == gameId && gc.ConfigId == configId);

        if (gameConfig == null)
        {
            // Create new relationship
            gameConfig = new GameConfig
            {
                GameId = gameId,
                ConfigId = configId,
                IsEnabled = enabled
            };
            _context.GameConfigs.Add(gameConfig);
        }
        else
        {
            // Update existing relationship
            gameConfig.IsEnabled = enabled;
        }

        game.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<string> EnsureUniquePathAsync(string path, int excludeGameId)
    {
        // Check if path is already unique (excluding current game)
        var existingGame = await _context.Games
            .FirstOrDefaultAsync(g => g.Path == path && g.Id != excludeGameId);

        if (existingGame == null)
        {
            // Path is unique, return as is
            return path;
        }

        // Path exists, find unique path by appending suffixes
        int suffix = 1;
        string uniquePath;
        do
        {
            uniquePath = $"{path}-x{suffix}";
            existingGame = await _context.Games
                .FirstOrDefaultAsync(g => g.Path == uniquePath && g.Id != excludeGameId);
            suffix++;
        } while (existingGame != null);

        return uniquePath;
    }
}

