using System.Diagnostics;
using KW_Calendar.Models;
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

    /*
    public void Initialize(int eventId)
    {
        _view.FavoriteToggleRequested += OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested += OnOpenExternalLinkRequested;
        _view.ViewClosed += OnViewClosed;

        _ = LoadEventAsync(eventId);
    }*/
    //수정
    public async Task InitializeAsync(int eventId)
    {
        _view.FavoriteToggleRequested += OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested += OnOpenExternalLinkRequested;
        _view.ViewClosed += OnViewClosed;

        await LoadEventAsync(eventId);
    }

    private async void OnFavoriteToggleRequested(object? sender, int id)
        => await ToggleFavoriteAsync(id);

    private void OnOpenExternalLinkRequested(object? sender, EventArgs e)
    {
        var link = _view.CurrentEvent?.ExternalLink;
        if (string.IsNullOrEmpty(link))
            return;

        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception)
        {
            // 브라우저 실행 실패 시 무시
        }
    }

    private void OnViewClosed(object? sender, EventArgs e)
    {
        // 뷰 재사용 시 메모리 누수 방지를 위해 구독 해제
        _view.FavoriteToggleRequested -= OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested -= OnOpenExternalLinkRequested;
        _view.ViewClosed -= OnViewClosed;
    }

    /*private async Task LoadEventAsync(int eventId)
    {
        var ev = await _eventService.GetEventByIdAsync(eventId);
        _view.CurrentEvent = ev ?? new Event();
        _view.IsFavorited = ev?.IsFavorited ?? false;
    }*/
    //수정
    private async Task LoadEventAsync(int eventId)
    {
        var ev = await _eventService.GetEventByIdAsync(eventId);

        if (ev == null)
            return;

        _view.CurrentEvent = ev;
    }


    private async Task ToggleFavoriteAsync(int eventId)
    {
        var updated = await _eventService.ToggleFavoriteAsync(eventId);
        //_view.IsFavorited = updated.IsFavorited;
        _view.CurrentEvent = updated;
    }
    
}

