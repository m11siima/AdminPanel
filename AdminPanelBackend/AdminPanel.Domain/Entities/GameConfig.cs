namespace AdminPanel.Domain.Entities;

public class GameConfig
{
    public int GameId { get; set; }
    public Guid ConfigId { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Game? Game { get; set; }
    public Config? Config { get; set; }
}

