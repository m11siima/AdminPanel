namespace AdminPanel.Responses;

public record PermissionResponse(
    string Key,
    string Resource,
    string Action,
    string? Description);

