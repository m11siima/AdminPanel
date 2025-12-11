using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class UpdateConfigRequest
{
    [MaxLength(128)]
    public string? Name { get; set; }

    public List<GameConfigItem>? Games { get; set; }
}

