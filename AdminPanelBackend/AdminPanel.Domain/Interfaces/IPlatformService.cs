using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Requests;

namespace AdminPanel.Domain.Interfaces;

public interface IPlatformService
{
    Task<IEnumerable<Platform>> GetPlatformsAsync();

    Task<Platform?> GetPlatformByIdAsync(Guid platformId);

    Task<Platform> CreatePlatformAsync(CreatePlatformRequest request);

    Task<Platform> UpdatePlatformAsync(Guid platformId, UpdatePlatformRequest request);

    Task DeletePlatformAsync(Guid platformId);
}

