namespace resumeSystem.Domain;

public class ResumeLike
{
    public int Id { get; set; }

    public int ResumeId { get; set; }

    public string UserId { get; set; } = null!;

    public Resume Resume { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}