using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class AssignRolesRequest
{
    [Required]
    public List<Guid> RoleIds { get; set; } = [];
}

