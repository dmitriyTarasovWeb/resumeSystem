using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public bool CanEdit { get; private set; }

    public bool CanView { get; private set; }

    public string? PhotoUrl { get; private set; }
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

        var isOwner = currentUser.Id == targetUser.Id;

        var isAdministrator =
            await _userManager.IsInRoleAsync(
                currentUser,
                "Administrator");

        var isRecruiter =
            await _userManager.IsInRoleAsync(
                currentUser,
                "Recruiter");

        CanView =
            isOwner ||
            isRecruiter ||
            isAdministrator;

        CanEdit =
            isOwner ||
            isAdministrator;

        if (!CanView)
        {
            return Forbid();
        }

        var requiredAttributes =
            await _dbContext.RequiredUserAttributes.FindAsync(targetUser.Id);

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
        PhotoUrl = requiredAttributes.PhotoUrl;

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

}