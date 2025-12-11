using System;

namespace AdminPanel.Responses;

public record PlatformResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Url,
    Guid ConfigId,
    string? ConfigName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

