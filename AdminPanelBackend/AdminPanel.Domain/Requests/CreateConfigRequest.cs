using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class CreateConfigRequest
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public List<int> GameIds { get; set; } = [];
}

