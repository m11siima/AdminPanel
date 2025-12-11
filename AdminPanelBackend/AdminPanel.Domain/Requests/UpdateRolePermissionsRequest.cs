using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class UpdateRolePermissionsRequest
{
    [Required]
    public List<string> PermissionKeys { get; set; } = [];
}

