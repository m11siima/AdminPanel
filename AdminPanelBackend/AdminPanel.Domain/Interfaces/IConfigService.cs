using AdminPanel.Domain.Entities;
using AdminPanel.Domain.Requests;

namespace AdminPanel.Domain.Interfaces;

public interface IConfigService
{
    Task<IEnumerable<Config>> GetConfigsAsync();
    Task<Config?> GetConfigByIdAsync(Guid configId);
    Task<Config> CreateConfigAsync(CreateConfigRequest request);
    Task<Config> UpdateConfigAsync(Guid configId, UpdateConfigRequest request);
    Task DeleteConfigAsync(Guid configId);
    Task<Domain.Requests.ConfigExportDto> ExportConfigAsync(Guid configId);
    Task<Config> ImportConfigAsync(Domain.Requests.ImportConfigRequest request);
    Task<ConfigFileExportResult> ExportConfigFileAsync(Guid configId);
    Task<Config> ImportConfigFileAsync(Stream fileStream, string fileName);
}

