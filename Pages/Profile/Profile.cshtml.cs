using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using resumeSystem.Domain;
using resumeSystem.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace resumeSystem.Pages.Profile;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly CloudinaryService _cloudinaryService;

    public ProfileModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        CloudinaryService cloudinaryService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; }

    [BindProperty]
    public IFormFile? Avatar { get; set; }

    [BindProperty]
    public Dictionary<int, string>? DynamicAttributes { get; set; } = new();

    [BindProperty]
    public List<ExperienceInputModel>? ExperiencesInput { get; set; } = new();

    public bool CanEdit { get; private set; }
    public bool CanView { get; private set; }
    public string? Role { get; private set; }
    public string? PhotoUrl { get; private set; }
    public List<ProfileAttributeViewModel> ProfileAttributes { get; set; } = new();
    public List<Category> AvailableCategories { get; set; } = new();
    public List<ExperienceViewModel> Experiences { get; set; } = new();
    public List<ResumeViewModel> Resumes { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        var (currentUser, targetUser, errorResult) = await GetUserContextAsync();
        if (errorResult != null) return errorResult;

        await LoadPageDataAsync(currentUser!, targetUser!);

        if (!CanView) return Forbid();

        var requiredAttributes = await _dbContext.RequiredUserAttributes.FindAsync(targetUser!.Id);
        if (requiredAttributes == null) return NotFound();

        Input = new InputModel
        {
            FirstName = requiredAttributes.Name,
            LastName = requiredAttributes.SecondName,
            Age = requiredAttributes.Age,
            Location = requiredAttributes.Location,
            Description = requiredAttributes.Description,
            Email = targetUser.Email ?? string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (currentUser, targetUser, errorResult) = await GetUserContextAsync();
        if (errorResult != null) return errorResult;

        if (!await HasEditPermissionAsync(currentUser!, targetUser!)) return Forbid();

        ModelState.Remove("");
        if (!ModelState.IsValid)
        {
            Input.Email = targetUser!.Email ?? string.Empty;
            await LoadPageDataAsync(currentUser!, targetUser!);
            return Page();
        }

        var requiredAttributes = await _dbContext.RequiredUserAttributes.FindAsync(targetUser!.Id);
        if (requiredAttributes == null) return NotFound();

        UpdateBasicInformation(requiredAttributes);

        await ProcessDynamicAttributesAsync(targetUser.Id);

        await ProcessExperiencesAsync(targetUser.Id);

        await _dbContext.SaveChangesAsync();

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync()
    {
        var (currentUser, targetUser, errorResult) = await GetUserContextAsync();
        if (errorResult != null) return errorResult;

        if (!await HasEditPermissionAsync(currentUser!, targetUser!)) return Forbid();

        if (Avatar == null)
        {
            ModelState.AddModelError(nameof(Avatar), "Выберите изображение.");
            return Page();
        }

        var requiredAttributes = await _dbContext.RequiredUserAttributes.FindAsync(targetUser!.Id);
        if (requiredAttributes == null) return NotFound();

        try
        {
            requiredAttributes.PhotoUrl = await _cloudinaryService.UploadAvatarAsync(Avatar);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            ModelState.AddModelError(nameof(Avatar), ex.Message);
            return Page();
        }

        return RedirectToPage(new { id = Id });
    }


    private async Task<(ApplicationUser? CurrentUser, ApplicationUser? TargetUser, IActionResult? ErrorResult)> GetUserContextAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return (null, null, NotFound());

        var targetUser = string.IsNullOrEmpty(Id)
            ? currentUser
            : await _userManager.FindByIdAsync(Id);

        if (targetUser == null) return (currentUser, null, NotFound());

        return (currentUser, targetUser, null);
    }


    private async Task<bool> HasEditPermissionAsync(ApplicationUser currentUser, ApplicationUser targetUser)
    {
        if (currentUser.Id == targetUser.Id) return true;
        return await _userManager.IsInRoleAsync(currentUser, "Administrator");
    }

    private void UpdateBasicInformation(RequiredUserAttributes requiredAttributes)
    {
        requiredAttributes.Name = Input.FirstName;
        requiredAttributes.SecondName = Input.LastName;
        requiredAttributes.Age = Input.Age;
        requiredAttributes.Location = Input.Location;
        requiredAttributes.Description = Input.Description;
    }

    private async Task ProcessDynamicAttributesAsync(string targetUserId)
    {
        DynamicAttributes ??= new Dictionary<int, string>();

        var existingUserAttributes = await _dbContext.UserAttributes
            .Where(ua => ua.UserId == targetUserId)
            .ToListAsync();

        foreach (var existingAttr in existingUserAttributes)
        {
            if (DynamicAttributes.TryGetValue(existingAttr.AttributeId, out var newValue))
            {
                existingAttr.Value = CleanCheckboxValue(newValue);
                DynamicAttributes.Remove(existingAttr.AttributeId);
            }
            else
            {
                _dbContext.UserAttributes.Remove(existingAttr);
            }
        }

        var newAttributes = DynamicAttributes
            .Where(attr => !string.IsNullOrWhiteSpace(attr.Value))
            .Select(attr => new UserAttribute
            {
                UserId = targetUserId,
                AttributeId = attr.Key,
                Value = CleanCheckboxValue(attr.Value)
            })
            .ToList();

        _dbContext.UserAttributes.AddRange(newAttributes);
    }

    private async Task ProcessExperiencesAsync(string targetUserId)
    {
        if (ExperiencesInput == null) return;

        var existingExperiences = await _dbContext.Experiences
            .Include(e => e.ExperienceTags)
            .Where(e => e.UserId == targetUserId)
            .ToListAsync();

        var inputIds = ExperiencesInput.Where(x => x.Id > 0).Select(x => x.Id).ToList();
        var toRemove = existingExperiences.Where(e => !inputIds.Contains(e.Id)).ToList();

        _dbContext.Experiences.RemoveRange(toRemove);

        foreach (var item in ExperiencesInput)
        {
            var utcStart = DateTime.SpecifyKind(item.StartDate, DateTimeKind.Utc);
            var utcEnd = item.EndDate.HasValue ? DateTime.SpecifyKind(item.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null;

            Experience exp;

            if (item.Id > 0)
            {
                exp = existingExperiences.FirstOrDefault(e => e.Id == item.Id)!;
                if (exp == null) continue;

                exp.CompanyName = item.CompanyName;
                exp.StartDate = utcStart;
                exp.EndDate = utcEnd;
                exp.Description = item.Description;

                _dbContext.ExperienceTags.RemoveRange(exp.ExperienceTags);
            }
            else
            {
                exp = new Experience
                {
                    UserId = targetUserId,
                    CompanyName = item.CompanyName,
                    StartDate = utcStart,
                    EndDate = utcEnd,
                    Description = item.Description
                };
                _dbContext.Experiences.Add(exp);
            }

            await ProcessExperienceTagsAsync(exp, item.Tags);
        }
    }

    private async Task ProcessExperienceTagsAsync(Experience exp, string? tagsInput)
    {
        if (string.IsNullOrWhiteSpace(tagsInput)) return;

        var tagTitles = tagsInput.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var title in tagTitles)
        {
            var tagInDb = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Title.ToLower() == title.ToLower());

            if (tagInDb == null)
            {
                tagInDb = new Tag { Title = title, IsDisplay = true };
                _dbContext.Tags.Add(tagInDb);
                await _dbContext.SaveChangesAsync();
            }

            _dbContext.ExperienceTags.Add(new ExperienceTag
            {
                Experience = exp,
                TagId = tagInDb.Id
            });
        }
    }

    private static string CleanCheckboxValue(string value) =>
        value == "false,true" || value == "true,false" ? "true" : value;

    private async Task LoadPageDataAsync(ApplicationUser currentUser, ApplicationUser targetUser)
    {
        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
        var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

        bool isAdministrator = currentUserRoles.Contains("Administrator");
        bool isRecruiter = currentUserRoles.Contains("Recruiter");
        bool isOwner = currentUser.Id == targetUser.Id;

        Role = targetUserRoles.Contains("Administrator") ? "Administrator"
             : targetUserRoles.Contains("Recruiter") ? "Recruiter"
             : "Candidate";

        CanView = isOwner || isRecruiter || isAdministrator;
        CanEdit = isOwner || isAdministrator;

        var requiredAttributes = await _dbContext.RequiredUserAttributes.FindAsync(targetUser.Id);
        PhotoUrl = requiredAttributes?.PhotoUrl;

        AvailableCategories = await _dbContext.Categories.Where(c => c.IsDisplay).ToListAsync();

        var attributes = await _dbContext.Attributes.Where(a => a.IsDisplay).ToListAsync();
        var attributeIds = attributes.Select(a => a.Id).ToList();

        var options = await _dbContext.AttributeOptions
            .Where(o => attributeIds.Contains(o.AttributeId))
            .ToListAsync();

        var userAttributes = await _dbContext.UserAttributes
            .Where(ua => ua.UserId == targetUser.Id)
            .ToListAsync();

        ProfileAttributes = attributes.Select(attr => new ProfileAttributeViewModel
        {
            AttributeId = attr.Id,
            CategoryId = attr.CategoryId,
            Title = attr.Title,
            DataTypeId = attr.DataTypeId,
            Value = userAttributes.FirstOrDefault(ua => ua.AttributeId == attr.Id)?.Value,
            Options = options.Where(o => o.AttributeId == attr.Id).Select(o => o.Options).ToList()
        }).ToList();

        var userExperiences = await _dbContext.Experiences
            .Include(e => e.ExperienceTags)
                .ThenInclude(et => et.Tag)
            .Where(e => e.UserId == targetUser.Id)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        Experiences = userExperiences.Select(e => new ExperienceViewModel
        {
            Id = e.Id,
            CompanyName = e.CompanyName,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Description = e.Description,
            Tags = e.ExperienceTags.Select(et => et.Tag.Title).ToList()
        }).ToList();

        var userResumes = await _dbContext.Resumes
            .Include(r => r.Position)
            .Where(r => r.UserId == targetUser.Id)
            .OrderBy(r => r.Position.Name)
            .ToListAsync();

        Resumes = userResumes
            .Select(r => new ResumeViewModel
            {
                Id = r.Id,
                PositionId = r.PositionId,
                PositionName = r.Position.Name
            })
            .ToList();

        var allTags = await _dbContext.Tags
            .Where(t => t.IsDisplay)
            .Select(t => t.Title)
            .ToListAsync();

        ViewData["AllTagsJson"] = JsonSerializer.Serialize(allTags);
    }

    public class ProfileAttributeViewModel
    {
        public int AttributeId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DataTypeId { get; set; }
        public string? Value { get; set; }
        public List<string> Options { get; set; } = new();
    }

    public class ExperienceInputModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
    }

    public class ExperienceViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class InputModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Возраст")]
        [Range(18, 100, ErrorMessage = "Возраст должен быть от 18 до 100 лет.")]
        public int? Age { get; set; }

        [Display(Name = "Местоположение")]
        [StringLength(200)]
        public string? Location { get; set; }

        [Display(Name = "О себе")]
        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов.")]
        public string? Description { get; set; }

        public string Email { get; set; } = string.Empty;
    }

    public class ResumeViewModel
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public Guid? VacancyId { get; set; }
    }
}