using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using resumeSystem.Data;
using resumeSystem.Domain;
using System.ComponentModel.DataAnnotations;
namespace resumeSystem.Pages.Profile;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly ApplicationDbContext _dbContext;
    public ProfileModel(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        var requiredAttributes =
            await _dbContext.RequiredUserAttributes.FindAsync(user.Id);

        if (requiredAttributes == null)
        {
            return NotFound();
        }

        Input.FirstName = requiredAttributes.Name;
        Input.LastName = requiredAttributes.SecondName;
        Input.Email = user.Email ?? string.Empty;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                Input.Email = currentUser.Email ?? string.Empty;
            }

            return Page();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        var requiredAttributes =
            await _dbContext.RequiredUserAttributes.FindAsync(user.Id);

        if (requiredAttributes == null)
        {
            return NotFound();
        }

        requiredAttributes.Name = Input.FirstName;
        requiredAttributes.SecondName = Input.LastName;

        await _dbContext.SaveChangesAsync();

        return RedirectToPage();
    }
}