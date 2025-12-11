using System;
using System.Collections.Generic;

namespace AdminPanel.Responses;

public record UserResponse(
    Guid Id,
    string Email,
    string? Name,
    IReadOnlyCollection<Guid> Roles);

