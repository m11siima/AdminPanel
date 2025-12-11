using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class UpdatePasswordRequest
{
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

