namespace resumeSystem.Domain;

public class ResumeExperience
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int ExperienceId { get; set; }

    public Resume Resume { get; set; } = null!;
    public Experience Experience { get; set; } = null!;
}