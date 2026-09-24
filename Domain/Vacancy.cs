namespace resumeSystem.Domain;

public class Vacancy
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public int PositionId { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Position Position { get; set; } = null!;

    public ICollection<VacancyTag> VacancyTags { get; set; } = new List<VacancyTag>();
    public ICollection<VacancyAttribute> VacancyAttributes { get; set; } = new List<VacancyAttribute>();
}