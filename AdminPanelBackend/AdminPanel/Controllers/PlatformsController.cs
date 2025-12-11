using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Domain.Security;
using AdminPanel.Infrastructure.Security;
using AdminPanel.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/platforms")]
[AuthorizePerm(Permissions.GM.Platforms.Read)]
public class PlatformsController(IPlatformService platformService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPlatforms()
    {
        var platforms = await platformService.GetPlatformsAsync();
        var responses = platforms.Select(p => p.ToResponse());
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlatform(Guid id)
    {
        var platform = await platformService.GetPlatformByIdAsync(id);
        if (platform == null)
        {
            return NotFound();
        }

        return Ok(platform.ToResponse());
    }

    [HttpPost]
    [AuthorizePerm(Permissions.GM.Platforms.Create)]
    public async Task<IActionResult> CreatePlatform([FromBody] CreatePlatformRequest request)
    {
        var platform = await platformService.CreatePlatformAsync(request);
        return CreatedAtAction(nameof(GetPlatform), new { id = platform.Id }, platform.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [AuthorizePerm(Permissions.GM.Platforms.Update)]
    public async Task<IActionResult> UpdatePlatform(Guid id, [FromBody] UpdatePlatformRequest request)
    {
        var platform = await platformService.UpdatePlatformAsync(id, request);
        return Ok(platform.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [AuthorizePerm(Permissions.GM.Platforms.Delete)]
    public async Task<IActionResult> DeletePlatform(Guid id)
    {
        await platformService.DeletePlatformAsync(id);
        return NoContent();
    }
}

