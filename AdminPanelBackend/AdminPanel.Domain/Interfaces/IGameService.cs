using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Requests;

namespace AdminPanel.Domain.Interfaces;

public interface IGameService
{
    Task<(IEnumerable<Game> Games, int TotalCount)> GetGamesAsync(GameListQuery query);

    Task<Game?> GetGameByIdAsync(int gameId);

    Task<Game> UpdateGameAsync(int gameId, UpdateGameRequest request);

    Task<Game> SetGameFeaturedAsync(int gameId, SetGameFeaturedRequest request);
}

