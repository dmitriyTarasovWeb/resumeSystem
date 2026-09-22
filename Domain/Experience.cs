namespace resumeSystem.Domain;

public class Experience
{
    public int Id { get; set; }

    public int? PositionId { get; set; }

    public string UserId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    public Position? Position { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ExperienceTag> ExperienceTags { get; set; } = new List<ExperienceTag>();
}