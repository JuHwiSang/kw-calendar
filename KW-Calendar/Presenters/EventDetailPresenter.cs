using KW_Calendar.Services;
using KW_Calendar.Views;

namespace KW_Calendar.Presenters;

public class EventDetailPresenter
{
    private readonly IEventDetailView _view;
    private readonly IEventService _eventService;

    public EventDetailPresenter(IEventDetailView view, IEventService eventService)
    {
        _view = view;
        _eventService = eventService;
    }

    public void Initialize(int eventId)
    {
        _view.FavoriteToggleRequested += OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested += OnOpenExternalLinkRequested;
        _view.ViewClosed += OnViewClosed;

        _ = LoadEventAsync(eventId);
    }

    private async void OnFavoriteToggleRequested(object? sender, int id)
        => await ToggleFavoriteAsync(id);

    private void OnOpenExternalLinkRequested(object? sender, EventArgs e)
    {
        // TODO: open _view.CurrentEvent?.ExternalLink in the default browser.
    }

    private void OnViewClosed(object? sender, EventArgs e)
    {
        // 뷰 재사용 시 메모리 누수 방지를 위해 구독 해제
        _view.FavoriteToggleRequested -= OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested -= OnOpenExternalLinkRequested;
        _view.ViewClosed -= OnViewClosed;
    }

    private async Task LoadEventAsync(int eventId)
    {
        // TODO: var ev = await _eventService.GetEventByIdAsync(eventId);
        // _view.CurrentEvent = ev;
        // _view.IsFavorited = ev?.IsFavorited ?? false;
    }

    private async Task ToggleFavoriteAsync(int eventId)
    {
        // TODO: var updated = await _eventService.ToggleFavoriteAsync(eventId);
        // _view.IsFavorited = updated.IsFavorited;
        // _view.CurrentEvent = updated;
    }
}
