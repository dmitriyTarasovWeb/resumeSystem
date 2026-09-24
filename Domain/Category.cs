namespace resumeSystem.Domain;

public class Category
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public bool IsDisplay { get; set; }
}