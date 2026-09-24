namespace resumeSystem.Domain;

public class Tag
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDisplay { get; set; } = true;

    public ICollection<ExperienceTag> ExperienceTags { get; set; } = new List<ExperienceTag>();
}