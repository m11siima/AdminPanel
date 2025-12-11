using AdminPanel.Domain.Entities;

namespace AdminPanel.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> permissionKeys, bool isSuperAdmin = false);

    string GenerateRefreshToken();
}

