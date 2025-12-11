namespace AdminPanel.Responses;

public record GameConfigItemResponse(
    int GameId,
    bool IsEnabled
);

public record ConfigResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<int> GameIds,
    IEnumerable<GameConfigItemResponse>? Games,
    IEnumerable<string> PlatformNames
);

