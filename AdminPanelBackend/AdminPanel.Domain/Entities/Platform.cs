namespace AdminPanel.Domain.Entities;

public class Platform
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Url { get; private set; }
    public Guid ConfigId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public Config? Config { get; private set; }

    private Platform() { }

    public static Platform Create(string name, string? description, string? url, Guid configId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Domain.Exceptions.ValidationException(nameof(name), "Platform name cannot be empty.");

        return new Platform
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Url = url?.Trim(),
            ConfigId = configId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? name, string? description, string? url, Guid? configId = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }
        if (description != null)
        {
            Description = description.Trim();
        }
        if (url != null)
        {
            Url = url.Trim();
        }
        if (configId.HasValue)
        {
            ConfigId = configId.Value;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignConfig(Guid configId)
    {
        ConfigId = configId;
        UpdatedAt = DateTime.UtcNow;
    }
}

