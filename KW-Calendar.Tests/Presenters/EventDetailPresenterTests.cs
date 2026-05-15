using KW_Calendar.Models;
using KW_Calendar.Presenters;
using KW_Calendar.Services;
using KW_Calendar.Views;
using NSubstitute;

namespace KW_Calendar.Tests.Presenters;

public class EventDetailPresenterTests
{
    private readonly IEventDetailView _view;
    private readonly IEventService _eventSvc;
    private readonly EventDetailPresenter _presenter;

    private static readonly Event DefaultEvent = new() { Id = 1, IsFavorited = false };
    private static readonly Event ToggledEvent = new() { Id = 1, IsFavorited = true };

    public EventDetailPresenterTests()
    {
        _view = Substitute.For<IEventDetailView>();
        _eventSvc = Substitute.For<IEventService>();

        _eventSvc.GetEventByIdAsync(Arg.Any<int>()).Returns(DefaultEvent);
        _eventSvc.ToggleFavoriteAsync(Arg.Any<int>()).Returns(ToggledEvent);

        _presenter = new EventDetailPresenter(_view, _eventSvc);
    }

    [Fact]
    public async Task Initialize_LoadsEvent()
    {
        _presenter.Initialize(1);
        await Task.Delay(100);

        await _eventSvc.Received(1).GetEventByIdAsync(1);
        _view.Received().CurrentEvent = DefaultEvent;
        _view.Received().IsFavorited = false;
    }

    [Fact]
    public async Task Initialize_FavoriteToggleRequested_CallsToggle()
    {
        _presenter.Initialize(1);
        await Task.Delay(50);

        _view.FavoriteToggleRequested += Raise.Event<EventHandler<int>>(this, 1);
        await Task.Delay(100);

        await _eventSvc.Received(1).ToggleFavoriteAsync(1);
    }

    [Fact]
    public async Task FavoriteToggle_UpdatesView()
    {
        _presenter.Initialize(1);
        await Task.Delay(50);

        _view.FavoriteToggleRequested += Raise.Event<EventHandler<int>>(this, 1);
        await Task.Delay(100);

        _view.Received().IsFavorited = true;
        _view.Received().CurrentEvent = ToggledEvent;
    }

    [Fact]
    public async Task ViewClosed_Unsubscribes_FavoriteToggleNoLongerCalled()
    {
        _presenter.Initialize(1);
        await Task.Delay(50);

        _view.ViewClosed += Raise.Event();
        _eventSvc.ClearReceivedCalls();

        _view.FavoriteToggleRequested += Raise.Event<EventHandler<int>>(this, 1);
        await Task.Delay(100);

        await _eventSvc.DidNotReceive().ToggleFavoriteAsync(Arg.Any<int>());
    }
}
