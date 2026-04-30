using KW_Calendar.Models;

namespace KW_Calendar.Views;

public interface IEventDetailView
{
    // --- Presenter가 설정 ---
    Event CurrentEvent { get; set; }
    bool IsFavorited { get; set; }

    // --- View가 발생, Presenter가 구독 ---
    event EventHandler<int> FavoriteToggleRequested;    // payload: event Id
    event EventHandler OpenExternalLinkRequested;
    event EventHandler ViewClosed;
}
