namespace resumeSystem.Domain;


using DomainAttribute = resumeSystem.Domain.Attribute;

public class UserAttribute
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    public DomainAttribute Attribute { get; set; } = null!;
}