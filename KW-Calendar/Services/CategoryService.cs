using KW_Calendar.Models;

namespace KW_Calendar.Services;

public class CategoryService : ICategoryService
{
    private readonly ILocalDbService _localDb;

    public CategoryService(ILocalDbService localDb) => _localDb = localDb;

    public Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken ct = default)
        => _localDb.GetAllCategoriesAsync(ct);

    public async Task<Category> ToggleCategoryFavoriteAsync(int categoryId, CancellationToken ct = default)
    {
        await _localDb.ToggleCategoryFavoriteAsync(categoryId, ct);
        // TODO: ILocalDbService에 GetCategoryByIdAsync 추가되면 전체 조회 제거 가능
        var all = await _localDb.GetAllCategoriesAsync(ct);
        return all.First(c => c.Id == categoryId);
    }

    public async Task<IReadOnlyList<Category>> GetFavoritedCategoriesAsync(CancellationToken ct = default)
    {
        var all = await _localDb.GetAllCategoriesAsync(ct);
        return all.Where(c => c.IsFavorited).ToList();
    }
}
