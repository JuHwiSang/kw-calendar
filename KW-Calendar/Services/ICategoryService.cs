using KW_Calendar.Models;

namespace KW_Calendar.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken ct = default);
    // TODO: 반환값 Category가 현재 호출부에서 사용되지 않음. void/bool로 변경 검토
    Task<Category> ToggleCategoryFavoriteAsync(int categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetFavoritedCategoriesAsync(CancellationToken ct = default);
}
