using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;

namespace resumeSystem.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;

    public List<VacancyListItem> Vacancies { get; set; } = new();

    public async Task OnGetAsync()
    {
        Vacancies = await _dbContext.Vacancies
            .Select(v => new VacancyListItem
            {
                Id = v.Id,
                Title = v.Title,
                PositionName = v.Position.Name,
                Description = v.Description
            })
            .OrderByDescending(v => v.Id)
            .ToListAsync();
    }

    public class VacancyListItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public IndexModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}