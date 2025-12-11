namespace AdminPanel.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType, object? entityId = null)
        : base($"Entity of type '{entityType}'{(entityId != null ? $" with id '{entityId}'" : "")} was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

