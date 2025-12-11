namespace AdminPanel.Responses;

public record GameResponse(
    int Id,
    string? Title,
    string? Path,
    string? Provider,
    string? Category,
    string? Tags,
    bool IsFeatured,
    bool Display,
    DateTime? CreatedAt,
    DateTime? UpdatedAt,
    IEnumerable<string> PlatformNames
);

