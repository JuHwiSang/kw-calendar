using System.Diagnostics;
using KW_Calendar.Models;
using KW_Calendar.Services;
using KW_Calendar.Views;

namespace KW_Calendar.Presenters;

public class EventDetailPresenter
{
    private readonly IEventDetailView _view;
    private readonly IEventService _eventService;

    /// <summary>토글이 DB까지 반영 완료된 후 발화. payload: 토글 후 Event 상태.</summary>
    public event EventHandler<Event>? FavoriteToggled;
    public event EventHandler<int>? EditRequested;
    public event EventHandler<int>? DeleteRequested;

    private EventHandler? _onEditRequested;
    private EventHandler? _onDeleteRequested;

    public EventDetailPresenter(IEventDetailView view, IEventService eventService)
    {
        _view = view;
        _eventService = eventService;
    }

    private int _eventId;

    public void Initialize(int eventId)
    {
        _eventId = eventId;
        _view.FavoriteToggleRequested += OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested += OnOpenExternalLinkRequested;
        _onEditRequested = (s, e) => EditRequested?.Invoke(this, eventId);
        _onDeleteRequested = OnDeleteRequested;
        _view.EditRequested += _onEditRequested;
        _view.DeleteRequested += _onDeleteRequested;
        _view.ViewClosed += OnViewClosed;

        _ = LoadEventAsync(eventId);
    }

    public Task ReloadAsync() => LoadEventAsync(_eventId);

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

    private async void OnDeleteRequested(object? sender, EventArgs e)
    {
        try
        {
            await _eventService.DeleteUserEventAsync(_view.CurrentEvent.Id);
            DeleteRequested?.Invoke(this, _view.CurrentEvent.Id);
            _view.CloseView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"삭제 중 오류가 발생했습니다.\n{ex.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnViewClosed(object? sender, EventArgs e)
    {
        // 뷰 재사용 시 메모리 누수 방지를 위해 구독 해제
        _view.FavoriteToggleRequested -= OnFavoriteToggleRequested;
        _view.OpenExternalLinkRequested -= OnOpenExternalLinkRequested;
        _view.EditRequested -= _onEditRequested;
        _view.DeleteRequested -= _onDeleteRequested;
        _view.ViewClosed -= OnViewClosed;
    }

    private async Task LoadEventAsync(int eventId)
    {
        var ev = await _eventService.GetEventByIdAsync(eventId);
        _view.CurrentEvent = ev ?? new Event();
        _view.IsFavorited = ev?.IsFavorited ?? false;
    }


    private async Task ToggleFavoriteAsync(int eventId)
    {
        var updated = await _eventService.ToggleFavoriteAsync(eventId);
        _view.IsFavorited = updated.IsFavorited;
        _view.CurrentEvent = updated;
        FavoriteToggled?.Invoke(this, updated);
    }

}

