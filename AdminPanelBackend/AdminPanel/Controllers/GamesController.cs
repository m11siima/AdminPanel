using AdminPanel.Domain.Interfaces;
using AdminPanel.Domain.Requests;
using AdminPanel.Domain.Security;
using AdminPanel.Infrastructure.Security;
using AdminPanel.Mappings;
using AdminPanel.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[ApiController]
[Route("api/games")]
[AuthorizePerm(Permissions.GM.Games.Read)]
public class GamesController(IGameService gameService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetGames([FromQuery] GameListQuery query)
    {
        var (games, totalCount) = await gameService.GetGamesAsync(query);
        var gameResponses = games.Select(g => g.ToResponse());
        
        return Ok(new GameListResponse(gameResponses, totalCount, query.Page, query.PageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGame(int id)
    {
        var game = await gameService.GetGameByIdAsync(id);
        if (game == null)
        {
            return NotFound();
        }

        return Ok(game.ToResponse());
    }

    [HttpPut("{id:int}")]
    [AuthorizePerm(Permissions.GM.Games.Edit)]
    public async Task<IActionResult> UpdateGame(int id, [FromBody] UpdateGameRequest request)
    {
        var game = await gameService.UpdateGameAsync(id, request);
        return Ok(game.ToResponse());
    }

    [HttpPut("{id:int}/featured")]
    [AuthorizePerm(Permissions.GM.Games.Feature)]
    public async Task<IActionResult> SetGameFeatured(int id, [FromBody] SetGameFeaturedRequest request)
    {
        var game = await gameService.SetGameFeaturedAsync(id, request);
        return Ok(game.ToResponse());
    }
}

