using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class CreatePlatformRequest
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Description { get; set; }

    [MaxLength(512)]
    public string? Url { get; set; }

    [Required]
    public Guid ConfigId { get; set; }
}

