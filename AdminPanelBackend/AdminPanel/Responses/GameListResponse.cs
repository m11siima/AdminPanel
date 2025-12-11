namespace AdminPanel.Responses;

public record GameListResponse(
    IEnumerable<GameResponse> Games,
    int TotalCount,
    int Page,
    int PageSize
);

