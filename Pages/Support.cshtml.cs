using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using resumeSystem.Domain;
using resumeSystem.Models;
using resumeSystem.Services;
using System.Text.Json;

namespace resumeSystem.Pages;

public class SupportModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DropboxService _dropboxService;

    public SupportModel(
        UserManager<ApplicationUser> userManager,
        DropboxService dropboxService)
    {
        _userManager = userManager;
        _dropboxService = dropboxService;
    }

    [BindProperty]
    public string Summary { get; set; } = string.Empty;

    [BindProperty]
    public string Priority { get; set; } = "Average";

    [BindProperty]
    public string SourceUrl { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(currentUser);

        var admins = await _userManager.GetUsersInRoleAsync("Administrator");

        var ticket = new SupportTicket
        {
            ReportedBy = currentUser.Email
                ?? currentUser.UserName
                ?? currentUser.Id,

            Role = roles.FirstOrDefault() ?? string.Empty,


            Link = SourceUrl,

            Priority = Priority,

            Summary = Summary,

            AdminEmails = admins
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .Select(x => x.Email!)
                .ToList()
        };

        var json = JsonSerializer.Serialize(
            ticket,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await _dropboxService.UploadFileAsync(
            $"support-ticket-{Guid.NewGuid()}.json",
            json);

        return new JsonResult(new
        {
            success = true
        });
    }
}