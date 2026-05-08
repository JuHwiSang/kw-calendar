using KW_Calendar.Models;
using KW_Calendar.Presenters;
using KW_Calendar.Services;
using KW_Calendar.Views;
using NSubstitute;

namespace KW_Calendar.Tests.Presenters;

public class CalendarPresenterTests
{
    private readonly ICalendarView _view;
    private readonly IEventService _eventSvc;
    private readonly ICategoryService _categorySvc;
    private readonly ISyncService _syncSvc;
    private readonly CalendarPresenter _presenter;

    public CalendarPresenterTests()
    {
        _view        = Substitute.For<ICalendarView>();
        _eventSvc    = Substitute.For<IEventService>();
        _categorySvc = Substitute.For<ICategoryService>();
        _syncSvc     = Substitute.For<ISyncService>();

        _view.DisplayedMonth.Returns(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));

        _eventSvc
            .GetEventsByDateRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Array.Empty<Event>());
        _eventSvc
            .GetFavoritedEventsAsync()
            .Returns(Array.Empty<Event>());
        _eventSvc
            .ToggleFavoriteAsync(Arg.Any<int>())
            .Returns(new Event());

        _categorySvc
            .GetAllCategoriesAsync()
            .Returns(Array.Empty<Category>());
        _categorySvc
            .ToggleCategoryFavoriteAsync(Arg.Any<int>())
            .Returns(new Category());

        _presenter = new CalendarPresenter(_view, _eventSvc, _categorySvc, _syncSvc);
    }

    [Fact]
    public async Task Initialize_LoadsEventsAndCategories()
    {
        _presenter.Initialize();
        await Task.Delay(100);

        await _eventSvc.Received(1)
            .GetEventsByDateRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>());
        await _eventSvc.Received(1).GetFavoritedEventsAsync();
        await _categorySvc.Received(1).GetAllCategoriesAsync();
    }

    [Fact]
    public async Task PreviousMonth_DecreasesDisplayedMonth()
    {
        var initial = new DateTime(2026, 5, 1);
        _view.DisplayedMonth.Returns(initial);

        _presenter.Initialize();
        _view.PreviousMonthRequested += Raise.Event();
        await Task.Delay(100);

        _view.Received().DisplayedMonth = initial.AddMonths(-1);
    }

    [Fact]
    public async Task NextMonth_IncreasesDisplayedMonth()
    {
        var initial = new DateTime(2026, 5, 1);
        _view.DisplayedMonth.Returns(initial);

        _presenter.Initialize();
        _view.NextMonthRequested += Raise.Event();
        await Task.Delay(100);

        _view.Received().DisplayedMonth = initial.AddMonths(1);
    }

    [Fact]
    public async Task EventFavoriteToggle_CallsToggleAndReloads()
    {
        _presenter.Initialize();
        await Task.Delay(50);

        _view.EventFavoriteToggleRequested += Raise.Event<EventHandler<int>>(this, 42);
        await Task.Delay(100);

        await _eventSvc.Received(1).ToggleFavoriteAsync(42);
    }

    [Fact]
    public async Task CategoryFavoriteToggle_CallsToggleAndReloads()
    {
        _presenter.Initialize();
        await Task.Delay(50);

        _view.CategoryFavoriteToggleRequested += Raise.Event<EventHandler<int>>(this, 7);
        await Task.Delay(100);

        await _categorySvc.Received(1).ToggleCategoryFavoriteAsync(7);
    }
}
