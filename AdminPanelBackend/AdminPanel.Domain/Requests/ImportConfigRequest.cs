using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class ImportConfigRequest
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public List<GameConfigItemDto> Games { get; set; } = new();
}


