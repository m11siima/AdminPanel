using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Domain.Requests;

public class UpdateUserRequest
{
    [MaxLength(256)]
    public string? Name { get; set; }

    [MinLength(6)]
    public string? Password { get; set; }

    public List<Guid>? RoleIds { get; set; }
}

