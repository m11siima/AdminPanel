using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class CreateRoleRequest
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Description { get; set; }

    public List<string> PermissionKeys { get; set; } = [];
}

