using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;

namespace resumeSystem.Pages.Resumes;

[Authorize(Roles = "Recruiter,Administrator")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;

    public IndexModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<ResumeViewModel> Resumes { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Resumes = await _dbContext.Resumes
            .Select(r => new ResumeViewModel
            {
                Id = r.Id,
                UserId = r.UserId,
                FirstName = r.User.RequiredUserAttributes != null
                    ? r.User.RequiredUserAttributes.Name
                    : string.Empty,
                LastName = r.User.RequiredUserAttributes != null
                    ? r.User.RequiredUserAttributes.SecondName
                    : string.Empty,
                PositionName = r.Position.Name
            })
            .OrderBy(r => r.LastName)
            .ThenBy(r => r.FirstName)
            .ThenBy(r => r.PositionName)
            .ToListAsync();

        return Page();
    }

    public class ResumeViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
    }
}