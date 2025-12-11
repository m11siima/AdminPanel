namespace AdminPanel.Domain.Entities;

public class Config
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<Platform> Platforms { get; private set; } = new List<Platform>();
    public ICollection<GameConfig> GameConfigs { get; private set; } = new List<GameConfig>();

    private Config() { }

    public static Config Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Domain.Exceptions.ValidationException(nameof(name), "Config name cannot be empty.");

        return new Config
        {
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Domain.Exceptions.ValidationException(nameof(name), "Config name cannot be empty.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignGame(int gameId, bool isEnabled = true)
    {
        if (GameConfigs.Any(gc => gc.GameId == gameId))
        {
            return;        }

        GameConfigs.Add(new GameConfig
        {
            GameId = gameId,
            ConfigId = Id,
            IsEnabled = isEnabled
        });
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignGames(IEnumerable<int> gameIds, bool isEnabled = true)
    {
        var gameIdsList = gameIds.Distinct().ToList();
        var existingGameIds = GameConfigs.Select(gc => gc.GameId).ToHashSet();

        var toRemove = GameConfigs.Where(gc => !gameIdsList.Contains(gc.GameId)).ToList();
        foreach (var gc in toRemove)
        {
            GameConfigs.Remove(gc);
        }

        foreach (var gameId in gameIdsList)
        {
            if (!existingGameIds.Contains(gameId))
            {
                GameConfigs.Add(new GameConfig
                {
                    GameId = gameId,
                    ConfigId = Id,
                    IsEnabled = isEnabled
                });
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateGames(IEnumerable<Domain.Requests.GameConfigItem> games)
    {
        var gamesList = games.ToList();
        var gameIdsSet = gamesList.Select(g => g.GameId).ToHashSet();
        var existingGameConfigs = GameConfigs.ToDictionary(gc => gc.GameId);

        var toRemove = GameConfigs.Where(gc => !gameIdsSet.Contains(gc.GameId)).ToList();
        foreach (var gc in toRemove)
        {
            GameConfigs.Remove(gc);
        }

        foreach (var gameItem in gamesList)
        {
            if (existingGameConfigs.TryGetValue(gameItem.GameId, out var existingGameConfig))
            {
                existingGameConfig.IsEnabled = gameItem.IsEnabled;
            }
            else
            {
                GameConfigs.Add(new GameConfig
                {
                    GameId = gameItem.GameId,
                    ConfigId = Id,
                    IsEnabled = gameItem.IsEnabled
                });
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasPlatforms()
    {
        return Platforms.Count > 0;
    }
}

