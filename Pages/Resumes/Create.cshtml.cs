using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using resumeSystem.Domain;

namespace resumeSystem.Pages.Resumes;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateModel(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [BindProperty]
    public List<int> SelectedExperienceIds { get; set; } = new();

    public ResumeVacancyViewModel Vacancy { get; set; } = null!;

    public RequiredUserAttributesViewModel RequiredAttributes { get; set; } = null!;

    public List<ResumeAttributeViewModel> Attributes { get; set; } = new();

    public List<ResumeExperienceViewModel> Experiences { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid vacancyId)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Challenge();
        }

        var vacancy = await _dbContext.Vacancies
            .Include(v => v.Position)
            .FirstOrDefaultAsync(v => v.Id == vacancyId);

        if (vacancy == null)
        {
            return NotFound();
        }

        var requiredAttributes = await _dbContext.RequiredUserAttributes
            .FirstOrDefaultAsync(x => x.UserId == currentUser.Id);

        if (requiredAttributes == null)
        {
            return NotFound();
        }

        var vacancyAttributes = await _dbContext.VacancyAttributes
            .Where(x => x.Vacancy.Id == vacancyId && x.Attribute.IsDisplay)
            .Select(x => new VacancyAttributeData
            {
                AttributeId = x.AttributeId,
                CategoryId = x.Attribute.CategoryId,
                Title = x.Attribute.Title,
                DataTypeId = x.Attribute.DataTypeId
            })
            .OrderBy(x => x.Title)
            .ToListAsync();

        var attributeIds = vacancyAttributes
            .Select(x => x.AttributeId)
            .ToList();

        var userAttributes = await _dbContext.UserAttributes
            .Where(x =>
                x.UserId == currentUser.Id &&
                attributeIds.Contains(x.AttributeId))
            .ToListAsync();

        var options = await _dbContext.AttributeOptions
            .Where(x => attributeIds.Contains(x.AttributeId))
            .Select(x => new AttributeOptionViewModel
            {
                AttributeId = x.AttributeId,
                Value = x.Options
            })
            .ToListAsync();


        var userExperiences = await _dbContext.Experiences
            .Include(e => e.ExperienceTags)
                .ThenInclude(et => et.Tag)
            .Where(e => e.UserId == currentUser.Id)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        Experiences = userExperiences
            .Select(e => new ResumeExperienceViewModel
            {
                Id = e.Id,
                CompanyName = e.CompanyName,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Description = e.Description,
                Tags = e.ExperienceTags
                    .Select(et => et.Tag.Title)
                    .ToList()
            })
            .ToList();

        Vacancy = new ResumeVacancyViewModel
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            PositionId = vacancy.PositionId,
            PositionName = vacancy.Position.Name,
            Description = vacancy.Description
        };

        RequiredAttributes = new RequiredUserAttributesViewModel
        {
            FirstName = requiredAttributes.Name,
            LastName = requiredAttributes.SecondName,
            PhotoUrl = requiredAttributes.PhotoUrl,
            Description = requiredAttributes.Description,
            Location = requiredAttributes.Location,
            Age = requiredAttributes.Age
        };

        Attributes = vacancyAttributes
            .Select(attribute => new ResumeAttributeViewModel
            {
                AttributeId = attribute.AttributeId,
                CategoryId = attribute.CategoryId,
                Title = attribute.Title,
                DataTypeId = attribute.DataTypeId,
                Value = userAttributes
                    .FirstOrDefault(x => x.AttributeId == attribute.AttributeId)
                    ?.Value,
                Options = options
                    .Where(x => x.AttributeId == attribute.AttributeId)
                    .Select(x => x.Value)
                    .ToList()
            })
            .ToList();

        return Page();
    }

    private class VacancyAttributeData
    {
        public int AttributeId { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DataTypeId { get; set; }
    }

    public class ResumeVacancyViewModel
    {
        public Guid Id { get; set; }

        public int PositionId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class RequiredUserAttributesViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }

        public int? Age { get; set; }
    }

    public class ResumeAttributeViewModel
    {
        public int AttributeId { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DataTypeId { get; set; }

        public string? Value { get; set; }

        public List<string> Options { get; set; } = new();
    }

    public class AttributeOptionViewModel
    {
        public int AttributeId { get; set; }

        public string Value { get; set; } = string.Empty;
    }


    public class ResumeExperienceViewModel
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}