using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using resumeSystem.Domain;

namespace resumeSystem.Pages.Vacancies;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public VacancyDetailsViewModel Vacancy { get; set; } = null!;

    public List<VacancyAttributeViewModel> Attributes { get; set; } = new();

    public List<string> Tags { get; set; } = new();

    public bool CanEdit =>
        User.IsInRole("Recruiter") ||
        User.IsInRole("Administrator");

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var vacancy = await _dbContext.Vacancies
            .Include(v => v.Position)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
        {
            return NotFound();
        }

        Vacancy = new VacancyDetailsViewModel
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Description = vacancy.Description,
            PositionName = vacancy.Position.Name
        };

        Attributes = await _dbContext.VacancyAttributes
            .Where(x => x.Vacancy.Id == id)
            .Select(x => new VacancyAttributeViewModel
            {
                Title = x.Attribute.Title,
                Value = x.AttributeValue,
                DataTypeName = x.Attribute.DataType.DataTypeName
            })
            .OrderBy(x => x.Title)
            .ToListAsync();

        Tags = await _dbContext.VacancyTags
            .Where(x => x.Vacancy.Id == id)
            .Select(x => x.Tag.Title)
            .OrderBy(x => x)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!User.IsInRole("Recruiter") &&
            !User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        var vacancy = await _dbContext.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy == null)
        {
            return NotFound();
        }

        var vacancyAttributes = await _dbContext.VacancyAttributes
            .Where(x => x.Vacancy.Id == id)
            .ToListAsync();

        if (vacancyAttributes.Count > 0)
        {
            _dbContext.VacancyAttributes.RemoveRange(
                vacancyAttributes);
        }

        var vacancyTags = await _dbContext.VacancyTags
            .Where(x => x.Vacancy.Id == id)
            .ToListAsync();

        if (vacancyTags.Count > 0)
        {
            _dbContext.VacancyTags.RemoveRange(
                vacancyTags);
        }

        _dbContext.Vacancies.Remove(vacancy);

        await _dbContext.SaveChangesAsync();

        return RedirectToPage("/Index");
    }

    public class VacancyDetailsViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class VacancyAttributeViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string DataTypeName { get; set; } = string.Empty;
    }
}