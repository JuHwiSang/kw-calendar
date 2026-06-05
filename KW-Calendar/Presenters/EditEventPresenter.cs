using KW_Calendar.Models;
using KW_Calendar.Services;
using KW_Calendar.Views;

namespace KW_Calendar.Presenters;

public class EditEventPresenter
{
    private readonly IAddEventView _view;
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly Event _existing;

    public bool WasSaved { get; private set; }

    public EditEventPresenter(
        IAddEventView view,
        IEventService eventService,
        ICategoryService categoryService,
        Event existing)
    {
        _view = view;
        _eventService = eventService;
        _categoryService = categoryService;
        _existing = existing;
        _view.Confirmed += OnConfirmed;
    }

    public async Task InitializeAsync()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        _view.SetCategories(categories);
        _view.SetTitle("일정 편집");
        _view.SetConfirmButtonText("수정");
        _view.SetEvent(_existing);
    }

    private async void OnConfirmed(object? sender, EventArgs e)
    {
        try
        {
            var updated = new Event
            {
                Id = _existing.Id,
                Title = _view.EventTitle,
                StartDt = _view.StartDt,
                EndDt = _view.EndDt,
                IsAllDay = _view.IsAllDay,
                Body = _view.Body,
                CategoryId = _view.CategoryId,
            };
            await _eventService.UpdateUserEventAsync(updated);
            WasSaved = true;
            _view.CloseView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"수정 중 오류가 발생했습니다.\n{ex.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
