using KW_Calendar.Models;
using KW_Calendar.Services;
using NSubstitute;

namespace KW_Calendar.Tests.Services;

public class EventServiceTests
{
    private readonly ILocalDbService _localDb;
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _localDb = Substitute.For<ILocalDbService>();
        _sut = new EventService(_localDb);
    }

    [Fact]
    public async Task GetEventsByDateRange_DelegatesToLocalDb()
    {
        var start = new DateTime(2026, 5, 1);
        var end = new DateTime(2026, 5, 31);
        var expected = new List<Event> { new() { Id = 1, Title = "A" } };
        _localDb.GetEventsByDateRangeAsync(start, end).Returns(expected);

        var result = await _sut.GetEventsByDateRangeAsync(start, end);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetEventById_DelegatesToLocalDb()
    {
        var ev = new Event { Id = 5, Title = "B" };
        _localDb.GetEventByIdAsync(5).Returns(ev);

        var result = await _sut.GetEventByIdAsync(5);

        Assert.Equal(ev, result);
    }

    [Fact]
    public async Task ToggleFavorite_CallsToggleAndReturnsEvent()
    {
        var ev = new Event { Id = 3, Title = "C", IsFavorited = true };
        _localDb.ToggleEventFavoriteAsync(3).Returns(true);
        _localDb.GetEventByIdAsync(3).Returns(ev);

        var result = await _sut.ToggleFavoriteAsync(3);

        await _localDb.Received(1).ToggleEventFavoriteAsync(3);
        Assert.Equal(ev, result);
    }

    [Fact]
    public async Task GetFavoritedEvents_DelegatesToLocalDb()
    {
        var expected = new List<Event> { new() { Id = 7, IsFavorited = true } };
        _localDb.GetFavoritedEventsAsync().Returns(expected);

        var result = await _sut.GetFavoritedEventsAsync();

        Assert.Equal(expected, result);
    }
}
