using KW_Calendar.Models;
using KW_Calendar.Services;
using KW_Calendar.Views;

namespace KW_Calendar.Presenters;

public class AddEventPresenter
{
    private readonly IAddEventView _view;
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;

    public AddEventPresenter(IAddEventView view, IEventService eventService, ICategoryService categoryService)
    {
        _view = view;
        _eventService = eventService;
        _categoryService = categoryService;
        _view.Confirmed += OnConfirmed;
    }

    public async Task InitializeAsync(DateOnly initialDate)
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        _view.SetCategories(categories);
        _view.SetInitialDate(initialDate);
    }

    private async void OnConfirmed(object? sender, EventArgs e)
    {
        try
        {
            var ev = new Event
            {
                Title = _view.EventTitle,
                StartDt = _view.StartDt,
                EndDt = _view.EndDt,
                IsAllDay = _view.IsAllDay,
                Body = _view.Body,
                CategoryId = _view.CategoryId,
            };
            await _eventService.AddUserEventAsync(ev);
            _view.CloseView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"일정 저장 중 오류가 발생했습니다.\n{ex.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
