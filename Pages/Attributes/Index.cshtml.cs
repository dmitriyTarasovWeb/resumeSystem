using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using resumeSystem.Data;
using resumeSystem.Domain;

using DomainAttribute = resumeSystem.Domain.Attribute;
using DomainDataType = resumeSystem.Domain.DataType;

namespace resumeSystem.Pages.Attributes;

[Authorize(Roles = "Recruiter,Administrator")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;

    public IndexModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Category> AvailableCategories { get; set; } = new();

    public List<DomainDataType> AvailableDataTypes { get; set; } = new();

    public List<AttributeViewModel> AvailableAttributes { get; set; } = new();

    [BindProperty]
    public CreateInputModel CreateInput { get; set; } = new();

    [BindProperty]
    public EditInputModel EditInput { get; set; } = new();

    [BindProperty]
    public string NewCategoryTitle { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    public bool IsEditMode => EditId.HasValue;

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadPageDataAsync();

        if (!EditId.HasValue)
        {
            return Page();
        }

        var attribute = await _dbContext.Attributes
            .FirstOrDefaultAsync(a => a.Id == EditId.Value && a.IsDisplay);

        if (attribute == null)
        {
            return NotFound();
        }

        EditInput = new EditInputModel
        {
            Id = attribute.Id,
            CategoryId = attribute.CategoryId,
            Title = attribute.Title
        };

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ModelState.Clear();

        await LoadPageDataAsync();

        if (!await ValidateCreateInputAsync())
        {
            return Page();
        }

        var title = CreateInput.Title.Trim();

        var existingAttribute = await _dbContext.Attributes
            .FirstOrDefaultAsync(a => a.Title.ToLower() == title.ToLower());

        if (existingAttribute != null)
        {
            if (existingAttribute.IsDisplay)
            {
                ModelState.AddModelError(
                    "CreateInput.Title",
                    "Атрибут с таким названием уже существует.");

                return Page();
            }

            existingAttribute.CategoryId = CreateInput.CategoryId;
            existingAttribute.Title = title;
            existingAttribute.DataTypeId = CreateInput.DataTypeId;
            existingAttribute.IsDisplay = true;

            await ReplaceAttributeOptionsAsync(
                existingAttribute.Id,
                CreateInput.DataTypeId,
                CreateInput.Options);

            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }

        var attribute = new DomainAttribute
        {
            CategoryId = CreateInput.CategoryId,
            Title = title,
            DataTypeId = CreateInput.DataTypeId,
            IsDisplay = true
        };

        _dbContext.Attributes.Add(attribute);

        await _dbContext.SaveChangesAsync();

        await ReplaceAttributeOptionsAsync(
            attribute.Id,
            CreateInput.DataTypeId,
            CreateInput.Options);

        await _dbContext.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        ModelState.Clear();

        EditId = EditInput.Id;

        await LoadPageDataAsync();

        if (!await ValidateEditInputAsync())
        {
            return Page();
        }

        var attribute = await _dbContext.Attributes
            .FirstOrDefaultAsync(a => a.Id == EditInput.Id && a.IsDisplay);

        if (attribute == null)
        {
            return NotFound();
        }

        attribute.CategoryId = EditInput.CategoryId;
        attribute.Title = EditInput.Title.Trim();

        await _dbContext.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        ModelState.Clear();

        var attribute = await _dbContext.Attributes
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attribute == null)
        {
            return NotFound();
        }

        attribute.IsDisplay = false;

        await _dbContext.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateCategoryAsync()
    {
        ModelState.Clear();

        await LoadPageDataAsync();

        if (string.IsNullOrWhiteSpace(NewCategoryTitle))
        {
            ModelState.AddModelError(
                nameof(NewCategoryTitle),
                "Введите название категории.");

            ViewData["ActiveTab"] = "categories";

            return Page();
        }

        var title = NewCategoryTitle.Trim();

        var categoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Title.ToLower() == title.ToLower());

        if (categoryExists)
        {
            ModelState.AddModelError(
                nameof(NewCategoryTitle),
                "Категория с таким названием уже существует.");

            ViewData["ActiveTab"] = "categories";

            return Page();
        }

        var category = new Category
        {
            Title = title,
            IsDisplay = true
        };

        _dbContext.Categories.Add(category);

        await _dbContext.SaveChangesAsync();

        ViewData["ActiveTab"] = "categories";

        return Page();
    }

    private async Task LoadPageDataAsync()
    {
        AvailableCategories = await _dbContext.Categories
            .Where(x => x.IsDisplay)
            .OrderBy(x => x.Title)
            .ToListAsync();

        AvailableDataTypes = await _dbContext.DataTypes
            .OrderBy(x => x.Id)
            .ToListAsync();

        var attributes = await _dbContext.Attributes
            .Where(x => x.IsDisplay)
            .OrderBy(x => x.Title)
            .ToListAsync();

        AvailableAttributes = attributes
            .Select(attribute => new AttributeViewModel
            {
                Id = attribute.Id,
                CategoryId = attribute.CategoryId,
                Title = attribute.Title,
                DataTypeId = attribute.DataTypeId
            })
            .ToList();
    }

    private async Task<bool> ValidateCreateInputAsync()
    {
        var isValid = true;

        if (CreateInput.CategoryId <= 0)
        {
            ModelState.AddModelError(
                "CreateInput.CategoryId",
                "Выберите категорию.");

            isValid = false;
        }
        else
        {
            var categoryExists = await _dbContext.Categories
                .AnyAsync(x =>
                    x.Id == CreateInput.CategoryId &&
                    x.IsDisplay);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    "CreateInput.CategoryId",
                    "Выбранная категория не найдена.");

                isValid = false;
            }
        }

        if (string.IsNullOrWhiteSpace(CreateInput.Title))
        {
            ModelState.AddModelError(
                "CreateInput.Title",
                "Введите название атрибута.");

            isValid = false;
        }

        if (CreateInput.DataTypeId <= 0)
        {
            ModelState.AddModelError(
                "CreateInput.DataTypeId",
                "Выберите тип данных.");

            isValid = false;
        }
        else
        {
            var dataTypeExists = await _dbContext.DataTypes
                .AnyAsync(x => x.Id == CreateInput.DataTypeId);

            if (!dataTypeExists)
            {
                ModelState.AddModelError(
                    "CreateInput.DataTypeId",
                    "Выбранный тип данных не найден.");

                isValid = false;
            }
        }

        if (CreateInput.DataTypeId == 7)
        {
            var options = NormalizeOptions(CreateInput.Options);

            if (options.Count == 0)
            {
                ModelState.AddModelError(
                    "CreateInput.Options",
                    "Для типа One of many добавьте хотя бы один вариант.");

                isValid = false;
            }
        }

        return isValid;
    }

    private async Task<bool> ValidateEditInputAsync()
    {
        var isValid = true;

        if (EditInput.Id <= 0)
        {
            ModelState.AddModelError(
                "EditInput.Id",
                "Не удалось определить атрибут.");

            isValid = false;
        }

        if (EditInput.CategoryId <= 0)
        {
            ModelState.AddModelError(
                "EditInput.CategoryId",
                "Выберите категорию.");

            isValid = false;
        }
        else
        {
            var categoryExists = await _dbContext.Categories
                .AnyAsync(x =>
                    x.Id == EditInput.CategoryId &&
                    x.IsDisplay);

            if (!categoryExists)
            {
                ModelState.AddModelError(
                    "EditInput.CategoryId",
                    "Выбранная категория не найдена.");

                isValid = false;
            }
        }

        if (string.IsNullOrWhiteSpace(EditInput.Title))
        {
            ModelState.AddModelError(
                "EditInput.Title",
                "Введите название атрибута.");

            isValid = false;
        }

        if (!isValid)
        {
            return false;
        }

        var title = EditInput.Title.Trim();

        var duplicate = await _dbContext.Attributes
            .AnyAsync(x =>
                x.Id != EditInput.Id &&
                x.IsDisplay &&
                x.Title.ToLower() == title.ToLower());

        if (duplicate)
        {
            ModelState.AddModelError(
                "EditInput.Title",
                "Атрибут с таким названием уже существует.");

            isValid = false;
        }

        return isValid;
    }

    private async Task ReplaceAttributeOptionsAsync(
        int attributeId,
        int dataTypeId,
        List<string>? options)
    {
        var existingOptions = await _dbContext.AttributeOptions
            .Where(x => x.AttributeId == attributeId)
            .ToListAsync();

        if (existingOptions.Count > 0)
        {
            _dbContext.AttributeOptions.RemoveRange(existingOptions);
        }

        if (dataTypeId != 7)
        {
            return;
        }

        var normalizedOptions = NormalizeOptions(options);

        if (normalizedOptions.Count == 0)
        {
            return;
        }

        var attributeOptions = normalizedOptions
            .Select(option => new AttributeOption
            {
                AttributeId = attributeId,
                Options = option
            })
            .ToList();

        _dbContext.AttributeOptions.AddRange(attributeOptions);
    }

    private static List<string> NormalizeOptions(List<string>? options)
    {
        if (options == null)
        {
            return new List<string>();
        }

        return options
            .Select(x => x?.Trim() ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public class AttributeViewModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DataTypeId { get; set; }
    }

    public class CreateInputModel
    {
        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DataTypeId { get; set; }

        public List<string> Options { get; set; } = new();
    }

    public class EditInputModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;
    }
}