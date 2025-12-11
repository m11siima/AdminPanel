using System;
using System.Collections.Generic;

namespace AdminPanel.Responses;

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    IReadOnlyCollection<string> PermissionKeys);

