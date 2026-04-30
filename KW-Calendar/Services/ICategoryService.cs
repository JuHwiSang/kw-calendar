using KW_Calendar.Models;

namespace KW_Calendar.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<Category> ToggleCategoryFavoriteAsync(int categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetFavoritedCategoriesAsync(CancellationToken ct = default);
}
