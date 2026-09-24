namespace resumeSystem.Domain;

public class VacancyTag
{
    public int Id { get; set; }
    public int VacancyId { get; set; }
    public int TagId { get; set; }

    public Vacancy Vacancy { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}