using resumeSystem.Domain;


namespace resumeSystem.Models;

public class VacancyApiToken
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int PositionId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public ApplicationUser User { get; set; } = null!;

    public Position Position { get; set; } = null!;
}