using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Domain;

namespace resumeSystem.Pages.Admin;

[Authorize(Roles = "Administrator")]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UsersModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public List<UserViewModel> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users
            .OrderBy(x => x.Email)
            .ToListAsync();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            Users.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty
            });
        }
    }

    public async Task<IActionResult> OnPostSetRoleAsync(
    List<string> selectedUserIds,
    string role)
    {
        if (selectedUserIds.Count == 0)
        {
            return RedirectToPage();
        }

        if (role != "Candidate" &&
            role != "Recruiter" &&
            role != "Administrator")
        {
            return BadRequest();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        foreach (var userId in selectedUserIds)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                continue;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

                if (!removeResult.Succeeded)
                {
                    continue;
                }
            }

            var addResult = await _userManager.AddToRoleAsync(
                user,
                role);

            if (!addResult.Succeeded)
            {
                continue;
            }

            if (currentUser != null &&
                user.Id == currentUser.Id)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        List<string> selectedUserIds)
    {
        if (selectedUserIds.Count == 0)
        {
            return RedirectToPage();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var deleteCurrentUser = false;

        foreach (var userId in selectedUserIds)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                continue;
            }

            if (user.Id == currentUser?.Id)
            {
                deleteCurrentUser = true;
            }

            await _userManager.DeleteAsync(user);
        }

        if (deleteCurrentUser)
        {
            await _signInManager.SignOutAsync();

            return RedirectToPage("/Account/Login");
        }

        return RedirectToPage();
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}