namespace resumeSystem.Models
{
    public class SupportTicket
    {
        public string ReportedBy { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> AdminEmails { get; set; } = new();
    }
}
