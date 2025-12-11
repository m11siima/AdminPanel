using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class UpdateGameRequest
{
    [MaxLength(256)]
    public string? Title { get; set; }

    [MaxLength(256)]
    public string? Path { get; set; }

    [MaxLength(128)]
    public string? Category { get; set; }

    [MaxLength(128)]
    public string? Tags { get; set; }
}

