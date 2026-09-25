namespace resumeSystem.Domain;

public class Resume
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public string UserId { get; set; } = null!;

    public Position Position { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<ResumeAttribute> ResumeAttributes { get; set; } = new List<ResumeAttribute>();
    public ICollection<VacancyResume> VacancyResumes { get; set; } = new List<VacancyResume>();
    public ICollection<ResumeExperience> ResumeExperiences { get; set; } = new List<ResumeExperience>();
}