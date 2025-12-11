using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Domain.Security;
using AdminPanel.Infrastructure.Security;
using AdminPanel.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/users")]
[AuthorizePerm(Permissions.Users.Read)]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userService.GetUsersAsync();
        var responses = users.Select(u => u.ToResponse());
        return Ok(responses);
    }

    [HttpPost]
    [AuthorizePerm(Permissions.Users.Create)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserCreated), new { id = user.Id }, user.ToResponse());
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetUserCreated(Guid id)
    {
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [AuthorizePerm(Permissions.Users.Update)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await userService.UpdateUserAsync(id, request);
        return Ok(user.ToResponse());
    }
}

