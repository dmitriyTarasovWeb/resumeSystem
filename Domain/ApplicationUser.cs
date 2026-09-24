using Microsoft.AspNetCore.Identity;

namespace resumeSystem.Domain;

public class ApplicationUser : IdentityUser
{
    public RequiredUserAttributes? RequiredUserAttributes { get; set; }
}