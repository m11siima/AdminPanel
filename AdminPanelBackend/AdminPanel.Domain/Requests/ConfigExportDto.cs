namespace AdminPanel.Domain.Requests;

public class ConfigExportDto
{
    public string Name { get; set; } = string.Empty;
    public List<GameConfigItemDto> Games { get; set; } = new();
}

public class GameConfigItemDto
{
    public int GameId { get; set; }
    public bool IsEnabled { get; set; }
}


