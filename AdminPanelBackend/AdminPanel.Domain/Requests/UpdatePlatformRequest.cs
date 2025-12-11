using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class UpdatePlatformRequest
{
    [MaxLength(128)]
    public string? Name { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    [MaxLength(512)]
    public string? Url { get; set; }

    public Guid? ConfigId { get; set; }
}

