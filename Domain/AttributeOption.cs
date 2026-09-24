namespace resumeSystem.Domain;

using DomainAttribute = resumeSystem.Domain.Attribute;

public class AttributeOption
{
    public int Id { get; set; }

    public int AttributeId { get; set; }

    public string Options { get; set; } = null!;

    public DomainAttribute Attribute { get; set; } = null!;
}