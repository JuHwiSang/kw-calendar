using KW_Calendar.Models;
using KW_Calendar.Services;

namespace KW_Calendar.Tests.Services;

public class LocalDbServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly LocalDbService _sut;

    public LocalDbServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"kwcal_test_{Guid.NewGuid():N}.db");
        _sut = new LocalDbService(_dbPath);
    }

    public void Dispose()
    {
        _sut.Dispose();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }

    // --- Events ---

    [Fact]
    public async Task UpsertEvents_InsertsNewEvent()
    {
        var ev = MakeEvent(1, "Test");
        await _sut.UpsertEventsAsync([ev]);

        var result = await _sut.GetEventByIdAsync(1);
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task UpsertEvents_PreservesIsFavorited()
    {
        await _sut.UpsertEventsAsync([MakeEvent(1, "A")]);
        await _sut.ToggleEventFavoriteAsync(1);  // IsFavorited = true

        // 서버에서 새 데이터 내려옴 (IsFavorited = false)
        await _sut.UpsertEventsAsync([MakeEvent(1, "A-updated")]);

        var result = await _sut.GetEventByIdAsync(1);
        Assert.True(result!.IsFavorited);
        Assert.Equal("A-updated", result.Title);
    }

    [Fact]
    public async Task ToggleEventFavorite_FlipsValue()
    {
        await _sut.UpsertEventsAsync([MakeEvent(1, "A")]);

        var first = await _sut.ToggleEventFavoriteAsync(1);
        var second = await _sut.ToggleEventFavoriteAsync(1);

        Assert.True(first);
        Assert.False(second);
    }

    [Fact]
    public async Task GetEventsByDateRange_ReturnsOverlappingEvents()
    {
        await _sut.UpsertEventsAsync([
            MakeEvent(1, "In",  new DateTime(2026, 5, 10), new DateTime(2026, 5, 12)),
            MakeEvent(2, "Out", new DateTime(2026, 6, 1),  new DateTime(2026, 6, 2)),
        ]);

        var result = await _sut.GetEventsByDateRangeAsync(
            new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));

        Assert.Single(result);
        Assert.Equal("In", result[0].Title);
    }
    [Fact]
    public async Task GetEventsByDateRange_ReturnsEventWithNullEndDt()
    {
        await _sut.UpsertEventsAsync([
            MakeEvent(1, "OneDay", new DateTime(2026, 5, 10), null),
    ]);

        var result = await _sut.GetEventsByDateRangeAsync(
            new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));

        Assert.Single(result);
        Assert.Equal("OneDay", result[0].Title);
        Assert.Null(result[0].EndDt);
    }

    [Fact]
    public async Task GetFavoritedEvents_IncludesEventFavorited()
    {
        await _sut.UpsertEventsAsync([MakeEvent(1, "Fav"), MakeEvent(2, "Not")]);
        await _sut.ToggleEventFavoriteAsync(1);

        var result = await _sut.GetFavoritedEventsAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetFavoritedEvents_IncludesFavoritedCategoryEvents()
    {
        await _sut.UpsertCategoriesAsync([new Category { Id = 10, Name = "Cat" }]);
        await _sut.ToggleCategoryFavoriteAsync(10);
        await _sut.UpsertEventsAsync([MakeEvent(1, "CatEvent", categoryId: 10)]);

        var result = await _sut.GetFavoritedEventsAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    // --- Categories ---

    [Fact]
    public async Task UpsertCategories_PreservesIsFavorited()
    {
        await _sut.UpsertCategoriesAsync([new Category { Id = 1, Name = "A" }]);
        await _sut.ToggleCategoryFavoriteAsync(1);

        await _sut.UpsertCategoriesAsync([new Category { Id = 1, Name = "A-updated" }]);

        var all = await _sut.GetAllCategoriesAsync();
        Assert.True(all[0].IsFavorited);
        Assert.Equal("A-updated", all[0].Name);
    }

    [Fact]
    public async Task ToggleCategoryFavorite_FlipsValue()
    {
        await _sut.UpsertCategoriesAsync([new Category { Id = 1, Name = "A" }]);

        var first = await _sut.ToggleCategoryFavoriteAsync(1);
        var second = await _sut.ToggleCategoryFavoriteAsync(1);

        Assert.True(first);
        Assert.False(second);
    }

    // --- Helpers ---

    private static Event MakeEvent(int id, string title,
        DateTime? start = null, DateTime? end = null, int categoryId = 1) => new()
        {
            Id = id,
            Title = title,
            StartDt = start ?? new DateTime(2026, 5, 10),
            EndDt = end,
            CategoryId = categoryId,
        };
}
