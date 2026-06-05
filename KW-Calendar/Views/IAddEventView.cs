using KW_Calendar.Models;

namespace KW_Calendar.Views;

public interface IAddEventView
{
    string EventTitle { get; }
    DateTime StartDt { get; }
    DateTime EndDt { get; }
    bool IsAllDay { get; }
    string? Body { get; }
    int CategoryId { get; }   // 0 = 미선택

    void SetCategories(IReadOnlyList<Category> categories);
    void SetInitialDate(DateOnly date);
    void SetEvent(Event e);
    void SetTitle(string title);
    void SetConfirmButtonText(string text);
    void CloseView();

    event EventHandler Confirmed;
}
