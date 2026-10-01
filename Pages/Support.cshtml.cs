using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using resumeSystem.Domain;
using resumeSystem.Models;
using System.Text.Json;

public class SupportModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SupportModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public string Summary { get; set; } = string.Empty;

    [BindProperty]
    public string Priority { get; set; } = "Average";

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Summary))
            return BadRequest();

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var roles = await _userManager.GetRolesAsync(currentUser);

        var admins = await _userManager.GetUsersInRoleAsync("Admin");

        var ticket = new SupportTicket
        {
            ReportedBy = currentUser.Email ?? currentUser.UserName ?? currentUser.Id,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Link = Request.GetDisplayUrl(),
            Priority = Priority,
            Summary = Summary,
            AdminEmails = admins
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .Select(x => x.Email!)
                .ToList()
        };

        var json = JsonSerializer.Serialize(ticket, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Content(json, "application/json");
    }
}