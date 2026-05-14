using PostService.Application.DTOs;
using PostService.Domain.Entities;
using Shared.Services;

namespace PostService.Application.Services;

/// <summary>
/// Category resolution logic — stays in PostService since Category is a Post concern.
/// No inter-service calls needed here.
/// </summary>
public class CategoryAppService
{
    private readonly BaseService<Category> _categoryService;

    public CategoryAppService(BaseService<Category> categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<(int? categoryId, string categoryName)> ResolveOrCreateAsync(int? providedCategoryId, string? customCategory)
    {
        if (providedCategoryId.HasValue && providedCategoryId.Value > 0)
        {
            var category = await _categoryService.GetAsync(providedCategoryId.Value);

            if (category != null)
                return (category.CategoryId, category.CategoryName);

            if (!string.IsNullOrWhiteSpace(customCategory))
                return await ReturnCreateCategory(customCategory);
        }

        if (string.IsNullOrWhiteSpace(customCategory))
            throw new InvalidOperationException(ErrorMessageForInvalidCategory(providedCategoryId));

        return await ReturnCreateCategory(customCategory);
    }

    private async Task<(int categoryId, string categoryName)> ReturnCreateCategory(string category)
    {
        var newCategory = await CreateCategoryAsync(category);
        return (newCategory.Id, newCategory.CategoryName);
    }

    private static string ErrorMessageForInvalidCategory(int? providedCategoryId) =>
        $"Categoría con {providedCategoryId} no existe. Proporcione CustomCategory para crear una nueva.";

    private async Task<CategoryDto.Response> CreateCategoryAsync(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new InvalidOperationException("CustomCategory no puede estar vacío");

        if (categoryName.Length > 200)
            throw new InvalidOperationException("CustomCategory no puede exceder 200 caracteres");

        var entity = Category.Create(categoryName);
        var created = await _categoryService.CreateAsync(entity);

        return new CategoryDto.Response
        {
            Id = created.CategoryId,
            CategoryName = created.CategoryName
        };
    }
}
