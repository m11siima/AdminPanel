namespace AdminPanel.Domain.Interfaces;

public interface IPermissionCatalogSeeder
{
    Task SyncAsync(CancellationToken cancellationToken = default);
}

