using KW_Calendar.Models;
using KW_Calendar.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace KW_Calendar.Tests.Services;

public class SyncServiceTests
{
    private readonly ISupabaseService _supabase;
    private readonly ILocalDbService _localDb;
    private readonly SyncService _sut;

    public SyncServiceTests()
    {
        _supabase = Substitute.For<ISupabaseService>();
        _localDb = Substitute.For<ILocalDbService>();
        _sut = new SyncService(_supabase, _localDb);

        _supabase.FetchCategoriesAsync().Returns(new List<Category>());
        _supabase.FetchEventsAsync().Returns(new List<Event>());
    }

    [Fact]
    public async Task SyncEvents_FetchesAndUpsertsCategoriesBeforeEvents()
    {
        var callOrder = new List<string>();
        _localDb.UpsertCategoriesAsync(Arg.Any<IReadOnlyList<Category>>())
            .Returns(_ => { callOrder.Add("categories"); return Task.CompletedTask; });
        _localDb.UpsertEventsAsync(Arg.Any<IReadOnlyList<Event>>())
            .Returns(_ => { callOrder.Add("events"); return Task.CompletedTask; });

        await _sut.SyncEventsAsync();

        Assert.Equal(new[] { "categories", "events" }, callOrder);
    }

    [Fact]
    public async Task SyncEvents_ReturnsEventCount()
    {
        _supabase.FetchEventsAsync().Returns(new List<Event>
        {
            new() { Id = 1, Title = "A" },
            new() { Id = 2, Title = "B" },
            new() { Id = 3, Title = "C" },
        });

        var result = await _sut.SyncEventsAsync();

        Assert.Equal(3, result);
    }

    [Fact]
    public async Task SyncEvents_WhenAlreadySyncing_ReturnsZeroAndSkips()
    {
        _supabase.FetchCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(async _ => { await Task.Delay(200); return (IReadOnlyList<Category>)new List<Category>(); });

        var first = _sut.SyncEventsAsync();
        await Task.Delay(50);

        var second = await _sut.SyncEventsAsync();

        Assert.Equal(0, second);
        await first;
    }

    [Fact]
    public async Task SyncEvents_IsSyncingTrueWhileRunning_FalseAfter()
    {
        _supabase.FetchCategoriesAsync(Arg.Any<CancellationToken>())
            .Returns(async _ => { await Task.Delay(200); return (IReadOnlyList<Category>)new List<Category>(); });

        var task = _sut.SyncEventsAsync();
        await Task.Delay(50);

        Assert.True(_sut.IsSyncing);

        await task;

        Assert.False(_sut.IsSyncing);
    }

    [Fact]
    public async Task SyncEvents_WhenFetchThrows_IsSyncingResetsToFalse()
    {
        _supabase.FetchCategoriesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network error"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.SyncEventsAsync());

        Assert.False(_sut.IsSyncing);
    }
}
