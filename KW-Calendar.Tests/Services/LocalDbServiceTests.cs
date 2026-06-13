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

    // --- User Events ---

    [Fact]
    public async Task AddUserEvent_AssignsNegativeId_AndSetsFlagsTrue()
    {
        var ev = new Event { Title = "My Event", StartDt = new DateTime(2026, 6, 1), CategoryId = 1 };
        await _sut.AddUserEventAsync(ev);

        var all = await _sut.GetEventsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
        var added = Assert.Single(all);
        Assert.True(added.Id < 0);
        Assert.True(added.IsFavorited);
        Assert.True(added.IsUserAdded);
    }

    [Fact]
    public async Task AddUserEvent_MultipleEvents_IdsDecrement()
    {
        var ev1 = new Event { Title = "First", StartDt = new DateTime(2026, 6, 1), CategoryId = 1 };
        var ev2 = new Event { Title = "Second", StartDt = new DateTime(2026, 6, 2), CategoryId = 1 };
        await _sut.AddUserEventAsync(ev1);
        await _sut.AddUserEventAsync(ev2);

        var all = await _sut.GetEventsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
        var ids = all.Select(e => e.Id).OrderBy(id => id).ToList();
        Assert.Equal(2, ids.Count);
        Assert.Equal(ids[0] + 1, ids[1]);
    }

    [Fact]
    public async Task DeleteUserEvent_RemovesRow()
    {
        var ev = new Event { Title = "To Delete", StartDt = new DateTime(2026, 6, 1), CategoryId = 1 };
        await _sut.AddUserEventAsync(ev);
        var added = (await _sut.GetEventsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue)).Single();

        await _sut.DeleteUserEventAsync(added.Id);

        var result = await _sut.GetEventByIdAsync(added.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteUserEvent_ThrowsForServerEvent()
    {
        await _sut.UpsertEventsAsync([MakeEvent(1, "Server Event")]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.DeleteUserEventAsync(1));
    }

    [Fact]
    public async Task UpdateUserEvent_UpdatesEditableFields()
    {
        var ev = new Event { Title = "Original", StartDt = new DateTime(2026, 6, 1), CategoryId = 1 };
        await _sut.AddUserEventAsync(ev);
        var added = (await _sut.GetEventsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue)).Single();

        var updated = new Event
        {
            Id = added.Id,
            Title = "Updated",
            StartDt = new DateTime(2026, 6, 2),
            EndDt = new DateTime(2026, 6, 3),
            IsAllDay = true,
            Body = "new body",
            CategoryId = 2,
        };
        await _sut.UpdateUserEventAsync(updated);

        var result = await _sut.GetEventByIdAsync(added.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated", result!.Title);
        Assert.Equal(new DateTime(2026, 6, 2), result.StartDt);
        Assert.Equal(new DateTime(2026, 6, 3), result.EndDt);
        Assert.True(result.IsAllDay);
        Assert.Equal("new body", result.Body);
        Assert.Equal(2, result.CategoryId);
    }

    [Fact]
    public async Task UpdateUserEvent_ThrowsForServerEvent()
    {
        await _sut.UpsertEventsAsync([MakeEvent(1, "Server Event")]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateUserEventAsync(MakeEvent(1, "Hacked")));
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
