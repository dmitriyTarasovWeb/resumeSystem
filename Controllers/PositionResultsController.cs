using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using System.Globalization;

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
            .Select(x => new VacancyResultDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ApplicationsCount = _dbContext.VacancyResumes
                    .Count(vr => vr.VacancyId == x.Id)
            })
            .ToListAsync();

        var applicationsPerVacancy = vacancies
            .Select(x => x.ApplicationsCount)
            .ToList();

        var vacanciesCount = vacancies.Count;

        var applicationsCount = applicationsPerVacancy.Sum();

        var averageApplicationsPerVacancy = vacanciesCount > 0
            ? applicationsCount / (double)vacanciesCount
            : 0;

        var minApplicationsPerVacancy = vacanciesCount > 0
            ? applicationsPerVacancy.Min()
            : 0;

        var maxApplicationsPerVacancy = vacanciesCount > 0
            ? applicationsPerVacancy.Max()
            : 0;

        var vacancyIds = vacancies
            .Select(x => x.Id)
            .ToList();

        var vacancyAttributes = await _dbContext.VacancyAttributes
            .Where(x => vacancyIds.Contains(x.VacancyId))
            .Select(x => new VacancyAttributeResultDto
            {
                AttributeId = x.AttributeId,
                Title = x.Attribute.Title,
                DataType = x.Attribute.DataType.DataTypeName,
                Value = x.AttributeValue
            })
            .ToListAsync();

        var attributes = BuildAttributeStatistics(vacancyAttributes);

        return Ok(new PositionResultsDto
        {
            Position = new PositionResultDto
            {
                Id = apiToken.Position.Id,
                Name = apiToken.Position.Name
            },

            Statistics = new PositionStatisticsDto
            {
                VacanciesCount = vacanciesCount,
                ApplicationsCount = applicationsCount,
                AverageApplicationsPerVacancy = averageApplicationsPerVacancy,
                MinApplicationsPerVacancy = minApplicationsPerVacancy,
                MaxApplicationsPerVacancy = maxApplicationsPerVacancy
            },

            Attributes = attributes,

            Vacancies = vacancies
        });
    }

    private static List<AttributeStatisticsDto> BuildAttributeStatistics(
        List<VacancyAttributeResultDto> values)
    {
        var result = new List<AttributeStatisticsDto>();

        var groups = values.GroupBy(x => new
        {
            x.AttributeId,
            x.Title,
            x.DataType
        });

        foreach (var group in groups)
        {
            var type = NormalizeDataType(group.Key.DataType);

            if (type == "number")
            {
                var numbers = new List<double>();

                foreach (var item in group)
                {
                    var number = ParseNumber(item.Value);

                    if (number.HasValue)
                    {
                        numbers.Add(number.Value);
                    }
                }

                result.Add(new AttributeStatisticsDto
                {
                    Title = group.Key.Title,
                    Type = "number",
                    Average = numbers.Count > 0
                        ? numbers.Average()
                        : null,
                    Min = numbers.Count > 0
                        ? numbers.Min()
                        : null,
                    Max = numbers.Count > 0
                        ? numbers.Max()
                        : null,
                    PopularValues = new List<PopularValueDto>()
                });

                continue;
            }

            var popularValues = group
                .Select(x => x.Value?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x!)
                .OrderByDescending(x => x.Count())
                .ThenBy(x => x.Key)
                .Take(5)
                .Select(x => new PopularValueDto
                {
                    Value = x.Key,
                    Count = x.Count()
                })
                .ToList();

            result.Add(new AttributeStatisticsDto
            {
                Title = group.Key.Title,
                Type = "text",
                Average = null,
                Min = null,
                Max = null,
                PopularValues = popularValues
            });
        }

        return result;
    }

    private static double? ParseNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();

        if (double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var invariantValue))
        {
            return invariantValue;
        }

        if (double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.GetCultureInfo("ru-RU"),
                out var russianValue))
        {
            return russianValue;
        }

        return null;
    }

    private static string NormalizeDataType(string? dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return "text";
        }

        var value = dataType.Trim().ToLowerInvariant();

        return value switch
        {
            "number" => "number",
            "numeric" => "number",
            "int" => "number",
            "integer" => "number",
            "long" => "number",
            "float" => "number",
            "double" => "number",
            "decimal" => "number",
            "число" => "number",
            "числовой" => "number",

            _ => "text"
        };
    }

    private sealed class PositionResultsDto
    {
        public PositionResultDto Position { get; set; } = new();
        public PositionStatisticsDto Statistics { get; set; } = new();
        public List<AttributeStatisticsDto> Attributes { get; set; } = new();
        public List<VacancyResultDto> Vacancies { get; set; } = new();
    }

    private sealed class PositionResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class PositionStatisticsDto
    {
        public int VacanciesCount { get; set; }
        public int ApplicationsCount { get; set; }
        public double AverageApplicationsPerVacancy { get; set; }
        public int MinApplicationsPerVacancy { get; set; }
        public int MaxApplicationsPerVacancy { get; set; }
    }

    private sealed class VacancyResultDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ApplicationsCount { get; set; }
    }

    private sealed class VacancyAttributeResultDto
    {
        public int AttributeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private sealed class AttributeStatisticsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public double? Average { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }

        public List<PopularValueDto> PopularValues { get; set; } = new();
    }

    private sealed class PopularValueDto
    {
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}