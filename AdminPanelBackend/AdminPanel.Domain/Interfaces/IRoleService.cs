using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Requests;

namespace AdminPanel.Domain.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetRolesAsync();
    Task<Role> CreateRoleAsync(CreateRoleRequest request);
    Task<Role> SetPermissionsAsync(Guid roleId, UpdateRolePermissionsRequest request);
}

