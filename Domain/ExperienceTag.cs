namespace resumeSystem.Domain;

public class ExperienceTag
{
    public int Id { get; set; }
    public int ExperienceId { get; set; }
    public int TagId { get; set; }

    public Experience Experience { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}