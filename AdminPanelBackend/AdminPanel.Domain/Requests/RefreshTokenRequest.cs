using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

