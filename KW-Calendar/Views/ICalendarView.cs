using KW_Calendar.Models;

namespace KW_Calendar.Views;

public interface ICalendarView
{
    // --- Presenter가 설정 ---
    DateTime DisplayedMonth { get; set; }
    IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay { get; set; }
    // bool IsSyncing { get; set; }  // TODO: 동기화 중 표시 (우선순위 낮음)

    // 전체 이벤트 목록 (즐겨찾기 필터링은 View가 자체적으로 처리)
    IReadOnlyList<Event> Events { get; set; }
    IReadOnlyList<Category> Categories { get; set; }

    // --- View가 발생, Presenter가 구독 ---

    // 캘린더 내비게이션
    event EventHandler PreviousMonthRequested;
    event EventHandler NextMonthRequested;
    // event EventHandler<DateOnly> DaySelected;        // TODO: 날짜 선택 (우선순위 낮음)
    event EventHandler<int> EventSelected;              // payload: event Id
    // event EventHandler<bool> ShowFavoritesOnlyChanged;  // TODO: 즐겨찾기만 보기 필터 (우선순위 낮음)
    // event EventHandler SyncRequested;               // TODO: 수동 동기화 버튼 (우선순위 낮음)

    // 즐겨찾기
    event EventHandler<int> EventFavoriteToggleRequested;     // payload: event Id
    event EventHandler<int> CategoryFavoriteToggleRequested;  // payload: category Id

    // 강인성 — 다른 인스턴스가 깨움 신호 보냄
    // TODO: X 버튼 → 트레이로 숨김은 View의 OnFormClosing 가로채기가 필요해 별도 티켓.
    event EventHandler OpenRequested;

    // --- Presenter가 명령 ---
    void Show();
    void Hide();
}
