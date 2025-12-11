using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Domain.Security;
using AdminPanel.Infrastructure.Security;
using AdminPanel.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/roles")]
[AuthorizePerm(Permissions.Roles.Read)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await roleService.GetRolesAsync();
        var responses = roles.Select(r => r.ToResponse());
        return Ok(responses);
    }

    [HttpPost]
    [AuthorizePerm(Permissions.Roles.Create)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = await roleService.CreateRoleAsync(request);
        return CreatedAtAction(nameof(CreateRole), new { id = role.Id }, role.ToResponse());
    }


    [HttpPut("{id:guid}/permissions")]
    [AuthorizePerm(Permissions.Roles.Update)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request)
    {
        var role = await roleService.SetPermissionsAsync(id, request);
        return Ok(role.ToResponse());
    }
}

