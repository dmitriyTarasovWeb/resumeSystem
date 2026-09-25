using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using resumeSystem.Domain;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;

namespace resumeSystem.Pages.Vacancies;

[Authorize(Roles = "Recruiter,Administrator")]
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
    public VacancyInputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    public bool IsEditMode => Id.HasValue;

    public List<Position> AvailablePositions { get; set; } = new();

    public List<Category> AvailableCategories { get; set; } = new();

    public List<VacancyAttributeViewModel> AvailableAttributes { get; set; } = new();



    public async Task<IActionResult> OnGetAsync()
    {
        if (Id.HasValue)
        {
            var vacancy = await _dbContext.Vacancies
                .Include(v => v.Position)
                .FirstOrDefaultAsync(v => v.Id == Id.Value);


            if (vacancy == null)
            {
                return NotFound();
            }

            Input.Title = vacancy.Title;
            Input.PositionId = vacancy.Position.Id;
            Input.PositionName = vacancy.Position.Name;
            Input.Description = vacancy.Description;


            Input.DynamicAttributes =
                await _dbContext.VacancyAttributes
                    .Where(x => x.Vacancy.Id == vacancy.Id)
                    .ToDictionaryAsync(
                        x => x.AttributeId,
                        x => x.AttributeValue);


            Input.Tags =
                string.Join(
                    ", ",
                    await _dbContext.VacancyTags
                        .Where(x => x.Vacancy.Id == vacancy.Id)
                        .Select(x => x.Tag.Title)
                        .OrderBy(x => x)
                        .ToListAsync());
        }

        await LoadPageDataAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        await LoadPageDataAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var position = await ResolvePositionAsync(Input.PositionName);

        if (position == null)
        {
            await LoadPageDataAsync();
            return Page();
        }

        var attributeValidationResult =
            await ValidateDynamicAttributesAsync();

        if (!attributeValidationResult)
        {
            await LoadPageDataAsync();
            return Page();
        }

        Vacancy vacancy;

        if (Id.HasValue)
        {
            vacancy = await _dbContext.Vacancies
                .FirstOrDefaultAsync(v => v.Id == Id.Value);

            if (vacancy == null)
            {
                return NotFound();
            }

            vacancy.Title =
                Input.Title.Trim();

            vacancy.Description =
                Input.Description;

            vacancy.Position =
                position;

            var existingVacancyAttributes =
                await _dbContext.VacancyAttributes
                    .Where(x => x.Vacancy.Id == vacancy.Id)
                    .ToListAsync();

            if (existingVacancyAttributes.Count > 0)
            {
                _dbContext.VacancyAttributes
                    .RemoveRange(existingVacancyAttributes);
            }

            var existingVacancyTags =
                await _dbContext.VacancyTags
                    .Where(x => x.Vacancy.Id == vacancy.Id)
                    .ToListAsync();

            if (existingVacancyTags.Count > 0)
            {
                _dbContext.VacancyTags
                    .RemoveRange(existingVacancyTags);
            }

            AddVacancyAttributes(vacancy);

            await AddVacancyTagsAsync(vacancy);
        }
        else
        {
            vacancy = new Vacancy
            {
                UserId = currentUser.Id,
                Position = position,
                Title = Input.Title.Trim(),
                Description = Input.Description
            };


            _dbContext.Vacancies.Add(vacancy);


            AddVacancyAttributes(vacancy);


            await AddVacancyTagsAsync(vacancy);
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToPage(
            "/Vacancies/Details",
            new { id = vacancy.Id });
    }

    private async Task<Position?> ResolvePositionAsync(string positionName)
    {
        var normalizedName = positionName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            ModelState.AddModelError("Input.PositionName", "Введите должность.");
            return null;
        }

        var position = await _dbContext.Positions
            .FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedName.ToLower());

        if (position != null)
        {
            return position;
        }

        position = new Position
        {
            Name = normalizedName
        };

        _dbContext.Positions.Add(position);

        return position;
    }

    private async Task<bool> ValidateDynamicAttributesAsync()
    {
        if (Input.DynamicAttributes == null ||
            Input.DynamicAttributes.Count == 0)
        {
            return true;
        }

        var attributeIds = Input.DynamicAttributes.Keys
            .Distinct()
            .ToList();

        var attributes = await _dbContext.Attributes
            .Where(a =>
                a.IsDisplay &&
                attributeIds.Contains(a.Id))
            .ToListAsync();

        if (attributes.Count != attributeIds.Count)
        {
            ModelState.AddModelError(
                "Input.DynamicAttributes",
                "Один или несколько выбранных атрибутов недоступны.");

            return false;
        }

        var options = await _dbContext.AttributeOptions
            .Where(o => attributeIds.Contains(o.AttributeId))
            .ToListAsync();

        var optionsByAttribute = options
            .GroupBy(o => o.AttributeId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Options).ToList());

        foreach (var item in Input.DynamicAttributes)
        {
            var attribute = attributes.First(
                a => a.Id == item.Key);

            var value = item.Value ?? string.Empty;

            value = CleanCheckboxValue(value);

            if (string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(
                    $"Input.DynamicAttributes[{attribute.Id}]",
                    $"Заполните атрибут «{attribute.Title}».");

                continue;
            }

            switch (attribute.DataTypeId)
            {
                case 1:
                case 2:
                    break;

                case 3:
                    if (!decimal.TryParse(
                            value,
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out _))
                    {
                        ModelState.AddModelError(
                            $"Input.DynamicAttributes[{attribute.Id}]",
                            $"Атрибут «{attribute.Title}» должен содержать число.");
                    }
                    break;

                case 4:
                    if (!DateTime.TryParseExact(
                            value,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out _))
                    {
                        ModelState.AddModelError(
                            $"Input.DynamicAttributes[{attribute.Id}]",
                            $"Некорректная дата в атрибуте «{attribute.Title}».");
                    }
                    break;

                case 5:
                    if (!ValidatePeriod(value))
                    {
                        ModelState.AddModelError(
                            $"Input.DynamicAttributes[{attribute.Id}]",
                            $"Некорректный период в атрибуте «{attribute.Title}».");
                    }
                    break;

                case 6:
                    if (value != "true" && value != "false")
                    {
                        ModelState.AddModelError(
                            $"Input.DynamicAttributes[{attribute.Id}]",
                            $"Некорректное значение атрибута «{attribute.Title}».");
                    }
                    break;

                case 7:
                    if (!optionsByAttribute.TryGetValue(
                            attribute.Id,
                            out var attributeOptions) ||
                        !attributeOptions.Contains(value))
                    {
                        ModelState.AddModelError(
                            $"Input.DynamicAttributes[{attribute.Id}]",
                            $"Недопустимое значение атрибута «{attribute.Title}».");
                    }
                    break;

                default:
                    ModelState.AddModelError(
                        $"Input.DynamicAttributes[{attribute.Id}]",
                        $"Неизвестный тип атрибута «{attribute.Title}».");
                    break;
            }
        }

        return ModelState.IsValid;
    }

    private static bool ValidatePeriod(string value)
    {
        var parts = value.Split('/');

        if (parts.Length != 2)
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                parts[0],
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var start))
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                parts[1],
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var end))
        {
            return false;
        }

        return start <= end;
    }

    private void AddVacancyAttributes(Vacancy vacancy)
    {
        if (Input.DynamicAttributes == null ||
            Input.DynamicAttributes.Count == 0)
        {
            return;
        }

        var vacancyAttributes = Input.DynamicAttributes
            .Select(item => new VacancyAttribute
            {
                Vacancy = vacancy,
                AttributeId = item.Key,
                AttributeValue = CleanCheckboxValue(
                    item.Value ?? string.Empty)
            })
            .ToList();

        _dbContext.VacancyAttributes.AddRange(vacancyAttributes);
    }

    private async Task AddVacancyTagsAsync(Vacancy vacancy)
    {
        var tagTitles = ParseTags(Input.Tags);

        if (tagTitles.Count == 0)
        {
            return;
        }

        var normalizedTitles = tagTitles
            .Select(x => x.ToLower())
            .Distinct()
            .ToList();

        var existingTags = await _dbContext.Tags
            .Where(t => normalizedTitles.Contains(t.Title.ToLower()))
            .ToListAsync();

        var tagsByTitle = existingTags
            .GroupBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First(),
                StringComparer.OrdinalIgnoreCase);

        var newTags = tagTitles
            .Where(title => !tagsByTitle.ContainsKey(title))
            .Select(title => new Tag
            {
                Title = title,
                IsDisplay = true
            })
            .ToList();

        if (newTags.Count > 0)
        {
            _dbContext.Tags.AddRange(newTags);

            foreach (var tag in newTags)
            {
                tagsByTitle[tag.Title] = tag;
            }
        }

        var vacancyTags = tagTitles
            .Select(title => new VacancyTag
            {
                Vacancy = vacancy,
                Tag = tagsByTitle[title]
            })
            .ToList();

        _dbContext.VacancyTags.AddRange(vacancyTags);
    }

    private async Task LoadPageDataAsync()
    {
        AvailablePositions = await _dbContext.Positions
            .Where(p => p.IsDisplay)
            .OrderBy(p => p.Name)
            .ToListAsync();

        AvailableCategories = await _dbContext.Categories
            .Where(c => c.IsDisplay)
            .OrderBy(c => c.Title)
            .ToListAsync();

        var attributes = await _dbContext.Attributes
            .Where(a => a.IsDisplay)
            .OrderBy(a => a.Title)
            .ToListAsync();

        var attributeIds = attributes
            .Select(a => a.Id)
            .ToList();

        var options = await _dbContext.AttributeOptions
            .Where(o => attributeIds.Contains(o.AttributeId))
            .ToListAsync();

        var currentValues = Input.DynamicAttributes
            ?? new Dictionary<int, string>();

        AvailableAttributes = attributes
            .Select(attribute => new VacancyAttributeViewModel
            {
                AttributeId = attribute.Id,
                CategoryId = attribute.CategoryId,
                Title = attribute.Title,
                DataTypeId = attribute.DataTypeId,
                Value = currentValues.TryGetValue(
                    attribute.Id,
                    out var value)
                    ? CleanCheckboxValue(value)
                    : null,
                Options = options
                    .Where(o => o.AttributeId == attribute.Id)
                    .Select(o => o.Options)
                    .ToList()
            })
            .ToList();

        var allTags = await _dbContext.Tags
            .Where(t => t.IsDisplay)
            .Select(t => t.Title)
            .ToListAsync();

        ViewData["AllTagsJson"] = JsonSerializer.Serialize(allTags);
    }

    private static List<string> ParseTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        return input
            .Split(
                new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string CleanCheckboxValue(string value)
    {
        return value == "false,true" ||
               value == "true,false"
            ? "true"
            : value;
    }

    public class VacancyInputModel
    {
        [Required(ErrorMessage = "Введите название вакансии.")]
        [StringLength(300)]
        [Display(Name = "Название вакансии")]
        public string Title { get; set; } = string.Empty;

        public int? PositionId { get; set; }
        public string? PositionName { get; set; }

        [Display(Name = "Описание вакансии")]
        public string? Description { get; set; }

        public Dictionary<int, string> DynamicAttributes { get; set; }
            = new();

        public string? Tags { get; set; }
    }

    public class VacancyAttributeViewModel
    {
        public int AttributeId { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DataTypeId { get; set; }

        public string? Value { get; set; }

        public List<string> Options { get; set; } = new();
    }
}