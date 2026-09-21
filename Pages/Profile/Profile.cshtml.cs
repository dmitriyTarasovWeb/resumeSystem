using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using resumeSystem.Domain;
using resumeSystem.Services;
using System.ComponentModel.DataAnnotations;

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
    public Dictionary<int, string> DynamicAttributes { get; set; } = new();

    public bool CanEdit { get; private set; }

    public bool CanView { get; private set; }

    public string? Role { get; set; }



    public string? PhotoUrl { get; private set; }
    public List<ProfileAttributeViewModel> ProfileAttributes { get; set; } = new();
    public List<Category> AvailableCategories { get; set; } = new();

    public class ProfileAttributeViewModel
    {
        public int AttributeId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DataTypeId { get; set; }
        public string? Value { get; set; }
        public List<string> Options { get; set; } = new();
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




    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return NotFound();
        }

        ApplicationUser? targetUser;

        if (string.IsNullOrEmpty(Id))
        {
            targetUser = currentUser;
        }
        else
        {
            targetUser = await _userManager.FindByIdAsync(Id);

            if (targetUser == null)
            {
                return NotFound();
            }
        }

        await LoadPageDataAsync(currentUser, targetUser);

        if (!CanView)
        {
            return Forbid();
        }

        var requiredAttributes = await _dbContext.RequiredUserAttributes.FindAsync(targetUser.Id);

        if (requiredAttributes == null)
        {
            return NotFound();
        }

        Input.FirstName = requiredAttributes.Name;
        Input.LastName = requiredAttributes.SecondName;
        Input.Age = requiredAttributes.Age;
        Input.Location = requiredAttributes.Location;
        Input.Description = requiredAttributes.Description;
        Input.Email = targetUser.Email ?? string.Empty;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return NotFound();
        }

        ApplicationUser? targetUser;

        if (string.IsNullOrEmpty(Id))
        {
            targetUser = currentUser;
        }
        else
        {
            targetUser = await _userManager.FindByIdAsync(Id);

            if (targetUser == null)
            {
                return NotFound();
            }
        }

        var isAdministrator =
            await _userManager.IsInRoleAsync(
                currentUser,
                "Administrator");

        var canEdit =
            currentUser.Id == targetUser.Id ||
            isAdministrator;

        if (!canEdit)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            Input.Email = targetUser.Email ?? string.Empty;

            await LoadPageDataAsync(currentUser, targetUser);

            return Page();
        }

        var requiredAttributes =
            await _dbContext.RequiredUserAttributes
                .FindAsync(targetUser.Id);

        if (requiredAttributes == null)
        {
            return NotFound();
        }

        requiredAttributes.Name = Input.FirstName;
        requiredAttributes.SecondName = Input.LastName;
        requiredAttributes.Age = Input.Age;
        requiredAttributes.Location = Input.Location;
        requiredAttributes.Description = Input.Description;
        var existingUserAttributes = await _dbContext.UserAttributes
            .Where(ua => ua.UserId == targetUser.Id)
            .ToListAsync();

        foreach (var existingAttr in existingUserAttributes)
        {
            if (DynamicAttributes.TryGetValue(existingAttr.AttributeId, out var newValue))
            {
                if (newValue == "false,true") newValue = "true";

                existingAttr.Value = newValue;

                DynamicAttributes.Remove(existingAttr.AttributeId);
            }
            else
            {
                _dbContext.UserAttributes.Remove(existingAttr);
            }
        }

        foreach (var newAttr in DynamicAttributes)
        {
            var finalValue = newAttr.Value;
            if (finalValue == "false,true") finalValue = "true";

            if (!string.IsNullOrWhiteSpace(finalValue))
            {
                _dbContext.UserAttributes.Add(new UserAttribute
                {
                    UserId = targetUser.Id,
                    AttributeId = newAttr.Key,
                    Value = finalValue
                });
            }
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return NotFound();
        }

        ApplicationUser? targetUser;

        if (string.IsNullOrEmpty(Id))
        {
            targetUser = currentUser;
        }
        else
        {
            targetUser = await _userManager.FindByIdAsync(Id);

            if (targetUser == null)
            {
                return NotFound();
            }
        }

        var isAdministrator =
            await _userManager.IsInRoleAsync(
                currentUser,
                "Administrator");

        var canEdit =
            currentUser.Id == targetUser.Id ||
            isAdministrator;

        if (!canEdit)
        {
            return Forbid();
        }

        if (Avatar == null)
        {
            ModelState.AddModelError(
                nameof(Avatar),
                "Выберите изображение.");

            return Page();
        }

        var requiredAttributes =
            await _dbContext.RequiredUserAttributes
                .FindAsync(targetUser.Id);

        if (requiredAttributes == null)
        {
            return NotFound();
        }

        try
        {
            var photoUrl =
                await _cloudinaryService.UploadAvatarAsync(Avatar);

            requiredAttributes.PhotoUrl = photoUrl;

            await _dbContext.SaveChangesAsync();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                nameof(Avatar),
                ex.Message);

            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                nameof(Avatar),
                ex.Message);

            return Page();
        }

        return RedirectToPage(new { id = Id });
    }


    private async Task LoadPageDataAsync(ApplicationUser currentUser, ApplicationUser targetUser)
    {
        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
        bool isAdministrator = currentUserRoles.Contains("Administrator");
        bool isRecruiter = currentUserRoles.Contains("Recruiter");

        var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

        Role = targetUserRoles.Contains("Administrator") ? "Administrator"
             : targetUserRoles.Contains("Recruiter") ? "Recruiter"
             : "Candidate";

        var isOwner = currentUser.Id == targetUser.Id;

        CanView = isOwner || isRecruiter || isAdministrator;
        CanEdit = isOwner || isAdministrator;

        var requiredAttributes = await _dbContext.RequiredUserAttributes.FindAsync(targetUser.Id);
        if (requiredAttributes != null)
        {
            PhotoUrl = requiredAttributes.PhotoUrl;
        }

        var categories = await _dbContext.Categories
            .Where(c => c.IsDisplay)
            .ToListAsync();

        var attributes = await _dbContext.Attributes
            .Where(a => a.IsDisplay)
            .ToListAsync();

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

        AvailableCategories = categories;
    }

}