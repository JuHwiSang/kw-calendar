using KW_Calendar.Models;

namespace KW_Calendar.Views;

public interface IEventDetailView
{
    // --- Presenter가 설정 ---
    Event CurrentEvent { get; set; }
    bool IsFavorited { get; set; } // TODO: Event 모델에 IsFavorited 속성이 존재하므로 제거 가능할 듯함

    void CloseView();

    // --- View가 발생, Presenter가 구독 ---
    event EventHandler<int> FavoriteToggleRequested;    // payload: event Id
    event EventHandler OpenExternalLinkRequested;
    event EventHandler EditRequested;
    event EventHandler DeleteRequested;
    event EventHandler ViewClosed;
}

