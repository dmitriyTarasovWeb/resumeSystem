namespace resumeSystem.Domain
{
    public class RequiredUserAttributes
    {
        public string UserId { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string SecondName { get; set; } = null!;

        public string? PhotoUrl { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }

        public int? Age { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
