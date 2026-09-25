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
    public RequiredUserAttributesViewModel RequiredAttributes { get; set; } = null!;

    [BindProperty]
    public Dictionary<int, string> DynamicAttributes { get; set; } = new();
    public ResumeVacancyViewModel Vacancy { get; set; } = null!;


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
            .Include(v => v.VacancyTags)
                .ThenInclude(vt => vt.Tag)
            .FirstOrDefaultAsync(v => v.Id == vacancyId);

        if (vacancy == null)
        {
            return NotFound();
        }

        var vacancyTagTitles = vacancy.VacancyTags
            .Select(vt => vt.Tag.Title)
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var userExperiences = await _dbContext.Experiences
            .Include(e => e.ExperienceTags)
                .ThenInclude(et => et.Tag)
            .Where(e => e.UserId == currentUser.Id)
            .ToListAsync();

        Experiences = userExperiences
            .Select(experience => new
            {
                Experience = experience,
                MatchCount = experience.ExperienceTags
                    .Select(et => et.Tag.Title)
                    .Count(title => vacancyTagTitles.Contains(title))
            })
            .Where(x => x.MatchCount > 0)
            .OrderByDescending(x => x.MatchCount)
            .Take(vacancy.MaxProjects)
            .Select(x => new ResumeExperienceViewModel
            {
                Id = x.Experience.Id,
                CompanyName = x.Experience.CompanyName,
                StartDate = x.Experience.StartDate,
                EndDate = x.Experience.EndDate,
                Description = x.Experience.Description,
                Tags = x.Experience.ExperienceTags
                    .Select(et => et.Tag.Title)
                    .ToList()
            })
            .ToList();



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


    public async Task<IActionResult> OnPostAsync(Guid vacancyId)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Challenge();
        }

        var vacancy = await _dbContext.Vacancies
            .Include(v => v.VacancyTags)
                .ThenInclude(vt => vt.Tag)
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

        requiredAttributes.Name =
            RequiredAttributes.FirstName?.Trim() ?? string.Empty;

        requiredAttributes.SecondName =
            RequiredAttributes.LastName?.Trim() ?? string.Empty;

        requiredAttributes.Age =
            RequiredAttributes.Age;

        requiredAttributes.Location =
            RequiredAttributes.Location?.Trim();

        requiredAttributes.Description =
            RequiredAttributes.Description;

        await ProcessResumeDynamicAttributesAsync(
            currentUser.Id,
            vacancyId);

        var vacancyTagTitles = vacancy.VacancyTags
            .Select(vt => vt.Tag.Title)
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var userExperiences = await _dbContext.Experiences
            .Include(e => e.ExperienceTags)
                .ThenInclude(et => et.Tag)
            .Where(e => e.UserId == currentUser.Id)
            .ToListAsync();

        var selectedExperiences = userExperiences
            .Select(experience => new
            {
                Experience = experience,
                MatchCount = experience.ExperienceTags
                    .Select(et => et.Tag.Title)
                    .Count(title => vacancyTagTitles.Contains(title))
            })
            .Where(x => x.MatchCount > 0)
            .OrderByDescending(x => x.MatchCount)
            .Take(vacancy.MaxProjects)
            .Select(x => x.Experience)
            .ToList();

        var resume = await _dbContext.Resumes
            .FirstOrDefaultAsync(r =>
                r.UserId == currentUser.Id &&
                r.PositionId == vacancy.PositionId);

        if (resume == null)
        {
            resume = new Resume
            {
                UserId = currentUser.Id,
                PositionId = vacancy.PositionId
            };

            _dbContext.Resumes.Add(resume);
        }

        var existingResumeExperiences = await _dbContext.ResumeExperiences
            .Where(x => x.ResumeId == resume.Id)
            .ToListAsync();

        if (existingResumeExperiences.Count > 0)
        {
            _dbContext.ResumeExperiences.RemoveRange(
                existingResumeExperiences);
        }

        var resumeExperiences = selectedExperiences
            .Select(experience => new ResumeExperience
            {
                Resume = resume,
                ExperienceId = experience.Id
            })
            .ToList();

        if (resumeExperiences.Count > 0)
        {
            _dbContext.ResumeExperiences.AddRange(
                resumeExperiences);
        }

        var vacancyResume = await _dbContext.VacancyResumes
            .FirstOrDefaultAsync(x =>
                x.ResumeId == resume.Id &&
                x.VacancyId == vacancy.Id);

        if (vacancyResume == null)
        {
            vacancyResume = new VacancyResume
            {
                Resume = resume,
                VacancyId = vacancy.Id,
                ApplyTime = DateTime.UtcNow
            };

            _dbContext.VacancyResumes.Add(vacancyResume);
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToPage(
            "/Vacancies/Details",
            new { id = vacancy.Id });
    }


    private async Task ProcessResumeDynamicAttributesAsync(
    string userId,
    Guid vacancyId)
    {
        DynamicAttributes ??= new Dictionary<int, string>();

        var vacancyAttributeIds = await _dbContext.VacancyAttributes
            .Where(x => x.Vacancy.Id == vacancyId)
            .Select(x => x.AttributeId)
            .ToListAsync();

        var postedAttributes = DynamicAttributes
            .Where(x => vacancyAttributeIds.Contains(x.Key))
            .ToDictionary(
                x => x.Key,
                x => x.Value ?? string.Empty);

        if (postedAttributes.Count == 0)
        {
            return;
        }

        var existingUserAttributes = await _dbContext.UserAttributes
            .Where(x =>
                x.UserId == userId &&
                postedAttributes.Keys.Contains(x.AttributeId))
            .ToListAsync();

        var existingByAttributeId = existingUserAttributes
            .ToDictionary(x => x.AttributeId);

        var newAttributes = new List<UserAttribute>();

        foreach (var item in postedAttributes)
        {
            var value = item.Value.Trim();

            if (existingByAttributeId.TryGetValue(item.Key, out var existingAttribute))
            {
                existingAttribute.Value = value;
            }
            else
            {
                newAttributes.Add(new UserAttribute
                {
                    UserId = userId,
                    AttributeId = item.Key,
                    Value = value
                });
            }
        }

        if (newAttributes.Count > 0)
        {
            _dbContext.UserAttributes.AddRange(newAttributes);
        }
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