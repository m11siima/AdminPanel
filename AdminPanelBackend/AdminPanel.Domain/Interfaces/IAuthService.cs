using AdminPanel.Domain.Requests;

namespace AdminPanel.Domain.Interfaces;

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginRequest request);

    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
}

