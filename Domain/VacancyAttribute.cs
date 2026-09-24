namespace resumeSystem.Domain;

using DomainAttribute = resumeSystem.Domain.Attribute;
public class VacancyAttribute
{
    public int Id { get; set; }
    public Guid VacancyId { get; set; }

    public int AttributeId { get; set; }

    public string AttributeValue { get; set; } = string.Empty;

    public Vacancy Vacancy { get; set; } = null!;

    public DomainAttribute Attribute { get; set; } = null!;
}