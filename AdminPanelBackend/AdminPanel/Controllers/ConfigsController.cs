using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Domain.Security;
using AdminPanel.Infrastructure.Security;
using AdminPanel.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/configs")]
[AuthorizePerm(Permissions.GM.Config.Read)]
public class ConfigsController(IConfigService configService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConfigs()
    {
        var configs = await configService.GetConfigsAsync();
        var responses = configs.Select(c => c.ToResponse());
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetConfig(Guid id)
    {
        var config = await configService.GetConfigByIdAsync(id);
        if (config == null)
        {
            return NotFound();
        }

        return Ok(config.ToResponse());
    }

    [HttpPost]
    [AuthorizePerm(Permissions.GM.Config.Create)]
    public async Task<IActionResult> CreateConfig([FromBody] CreateConfigRequest request)
    {
        var config = await configService.CreateConfigAsync(request);
        return CreatedAtAction(nameof(GetConfig), new { id = config.Id }, config.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [AuthorizePerm(Permissions.GM.Config.Update)]
    public async Task<IActionResult> UpdateConfig(Guid id, [FromBody] UpdateConfigRequest request)
    {
        var config = await configService.UpdateConfigAsync(id, request);
        return Ok(config.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [AuthorizePerm(Permissions.GM.Config.Delete)]
    public async Task<IActionResult> DeleteConfig(Guid id)
    {
        await configService.DeleteConfigAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/export")]
    [AuthorizePerm(Permissions.GM.Config.Export)]
    public async Task<IActionResult> ExportConfig(Guid id)
    {
        var exportFile = await configService.ExportConfigFileAsync(id);
        return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
    }

    [HttpPost("import")]
    [AuthorizePerm(Permissions.GM.Config.Import)]
    public async Task<IActionResult> ImportConfig(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var stream = file.OpenReadStream();
        var config = await configService.ImportConfigFileAsync(stream, file.FileName);
        return CreatedAtAction(nameof(GetConfig), new { id = config.Id }, config.ToResponse());
    }
}

