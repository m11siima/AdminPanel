using AdminPanel.Domain.Security;
using AdminPanel.Infrastructure.Persistence;
using AdminPanel.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCatalog()
    {
        var permissions = await context.Permissions
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionResponse(
                p.Key,
                p.Resource,
                p.Action,
                p.Description))
            .ToListAsync();

        return Ok(permissions);
    }

    [HttpGet("groups")]
    public IActionResult GetGroups()
    {
        var groups = new
        {
            GameManagement = PermissionGroups.GameManagement,
            UserManagement = PermissionGroups.UserManagement,
            RoleManagement = PermissionGroups.RoleManagement,
            FullAdmin = PermissionGroups.FullAdmin
        };

        return Ok(groups);
    }
}

