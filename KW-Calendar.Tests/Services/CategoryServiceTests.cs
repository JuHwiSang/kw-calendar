using KW_Calendar.Models;
using KW_Calendar.Services;
using NSubstitute;

namespace KW_Calendar.Tests.Services;

public class CategoryServiceTests
{
    private readonly ILocalDbService _localDb;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _localDb = Substitute.For<ILocalDbService>();
        _sut = new CategoryService(_localDb);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsAllFromLocalDb()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" },
        };
        _localDb.GetAllCategoriesAsync().Returns(categories);

        var result = await _sut.GetAllCategoriesAsync();

        Assert.Equal(categories, result);
    }

    [Fact]
    public async Task ToggleFavorite_CallsLocalDbToggle()
    {
        _localDb.ToggleCategoryFavoriteAsync(7).Returns(true);
        _localDb.GetAllCategoriesAsync().Returns(new List<Category> { new() { Id = 7, Name = "X" } });

        await _sut.ToggleCategoryFavoriteAsync(7);

        await _localDb.Received(1).ToggleCategoryFavoriteAsync(7);
    }

    [Fact]
    public async Task ToggleFavorite_ReturnsMatchingCategory()
    {
        var target = new Category { Id = 3, Name = "Target", IsFavorited = true };
        _localDb.ToggleCategoryFavoriteAsync(3).Returns(true);
        _localDb.GetAllCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = 1, Name = "Other" },
            target,
        });

        var result = await _sut.ToggleCategoryFavoriteAsync(3);

        Assert.Equal(target, result);
    }

    [Fact]
    public async Task ToggleFavorite_ThrowsWhenIdNotFound()
    {
        _localDb.ToggleCategoryFavoriteAsync(99).Returns(false);
        _localDb.GetAllCategoriesAsync().Returns(new List<Category> { new() { Id = 1, Name = "A" } });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ToggleCategoryFavoriteAsync(99));
    }

    [Fact]
    public async Task GetFavorited_ReturnsOnlyFavoritedCategories()
    {
        _localDb.GetAllCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = 1, Name = "A", IsFavorited = true  },
            new() { Id = 2, Name = "B", IsFavorited = false },
            new() { Id = 3, Name = "C", IsFavorited = true  },
        });

        var result = await _sut.GetFavoritedCategoriesAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.True(c.IsFavorited));
    }

    [Fact]
    public async Task GetFavorited_ReturnsEmptyWhenNoneFavorited()
    {
        _localDb.GetAllCategoriesAsync().Returns(new List<Category>
        {
            new() { Id = 1, Name = "A", IsFavorited = false },
        });

        var result = await _sut.GetFavoritedCategoriesAsync();

        Assert.Empty(result);
    }
}
