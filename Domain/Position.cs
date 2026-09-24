namespace resumeSystem.Domain;

public class Position
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDisplay { get; set; } = true;

    public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
}