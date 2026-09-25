namespace resumeSystem.Domain;

public class VacancyResume
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public Guid VacancyId { get; set; }
    public DateTime ApplyTime { get; set; }

    public Resume Resume { get; set; } = null!;
    public Vacancy Vacancy { get; set; } = null!;
}