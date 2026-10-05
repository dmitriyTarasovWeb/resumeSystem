using Microsoft.AspNetCore.Identity;
using resumeSystem.Models;

namespace resumeSystem.Domain;

public class ApplicationUser : IdentityUser
{
    public RequiredUserAttributes? RequiredUserAttributes { get; set; }
    public ICollection<VacancyApiToken> VacancyApiTokens { get; set; } = new List<VacancyApiToken>();
}