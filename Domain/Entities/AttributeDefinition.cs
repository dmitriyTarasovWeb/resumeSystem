using resumeSystem.Domain.Enums;

namespace resumeSystem.Domain.Entities;

public class AttributeDefinition
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Category { get; set; } = null!;

    public AttributeDataType DataType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public uint Version { get; set; }
}