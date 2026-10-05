using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;

namespace resumeSystem.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionResultsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public PositionResultsController(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResults([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new
            {
                message = "Token is required"
            });
        }

        var apiToken = await _dbContext.VacancyApiTokens
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x =>
                x.Token == token &&
                x.IsActive);

        if (apiToken == null)
        {
            return Unauthorized(new
            {
                message = "Invalid API token"
            });
        }

        var positionId = apiToken.PositionId;

        var vacancies = await _dbContext.Vacancies
            .Where(x => x.PositionId == positionId)
            .Select(x => new
            {
                id = x.Id,
                title = x.Title,
                description = x.Description
            })
            .ToListAsync();

        return Ok(new
        {
            position = new
            {
                id = apiToken.Position.Id,
                name = apiToken.Position.Name
            },

            statistics = new
            {
                vacanciesCount = vacancies.Count
            },

            vacancies
        });
    }
}