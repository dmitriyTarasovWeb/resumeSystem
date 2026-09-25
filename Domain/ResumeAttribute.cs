namespace resumeSystem.Domain;

public class ResumeAttribute
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int UserAttributeId { get; set; }

    public Resume Resume { get; set; } = null!;
    public UserAttribute UserAttribute { get; set; } = null!;
}