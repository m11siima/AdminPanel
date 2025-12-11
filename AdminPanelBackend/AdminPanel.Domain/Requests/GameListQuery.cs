namespace AdminPanel.Domain.Requests;

public class GameListQuery
{
    public string? Search { get; set; }
    public string? SearchTitle { get; set; }
    public string? SearchPath { get; set; }
    public string? Provider { get; set; }
    public string? Category { get; set; }
    public string? SortBy { get; set; } 
    public string? SortOrder { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

