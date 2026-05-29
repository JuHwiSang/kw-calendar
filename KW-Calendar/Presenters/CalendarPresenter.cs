using KW_Calendar.Models;
using KW_Calendar.Services;
using KW_Calendar.Views;

namespace KW_Calendar.Presenters;

public class CalendarPresenter
{
    private readonly ICalendarView _view;
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly ISyncService _syncService;

    // private bool _showFavoritesOnly;  // TODO: 즐겨찾기만 보기 필터 (우선순위 낮음)

    public CalendarPresenter(
        ICalendarView view,
        IEventService eventService,
        ICategoryService categoryService,
        ISyncService syncService)
    {
        _view = view;
        _eventService = eventService;
        _categoryService = categoryService;
        _syncService = syncService;
    }

    public void Initialize()
    {
        _view.PreviousMonthRequested += OnPreviousMonthRequested;
        _view.NextMonthRequested += OnNextMonthRequested;
        // _view.DaySelected += OnDaySelected;              // TODO: 날짜 선택 (우선순위 낮음)
        _view.EventSelected += OnEventSelected;
        // _view.ShowFavoritesOnlyChanged += OnShowFavoritesOnlyChanged;  // TODO: 즐겨찾기만 보기 필터 (우선순위 낮음)
        _view.SyncRequested += OnSyncRequested;          
        _view.EventFavoriteToggleRequested += OnEventFavoriteToggleRequested;
        _view.CategoryFavoriteToggleRequested += OnCategoryFavoriteToggleRequested;

        _view.DisplayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _ = LoadAllAsync();
    }

    // --- 캘린더 내비게이션 ---

    private async void OnPreviousMonthRequested(object? sender, EventArgs e)
        => await NavigateMonthAsync(-1);

    private async void OnNextMonthRequested(object? sender, EventArgs e)
        => await NavigateMonthAsync(1);

    // private void OnDaySelected(object? sender, DateOnly date) { }  // TODO: 날짜 선택 (우선순위 낮음)

    private async void OnEventSelected(object? sender, int eventId)
        => await OpenEventDetailAsync(eventId);

    // TODO: 즐겨찾기만 보기 필터 (우선순위 낮음)
    // private async void OnShowFavoritesOnlyChanged(object? sender, bool showFavoritesOnly)
    // {
    //     _showFavoritesOnly = showFavoritesOnly;
    //     await LoadEventsForCurrentMonthAsync();
    // }

    
     private async void OnSyncRequested(object? sender, EventArgs e)
        => await SyncAndRefreshAsync();

    // --- 즐겨찾기 ---

    private async void OnEventFavoriteToggleRequested(object? sender, int eventId)
    {
        await _eventService.ToggleFavoriteAsync(eventId);
        await LoadAllAsync();
    }

    private async void OnCategoryFavoriteToggleRequested(object? sender, int categoryId)
    {
        await _categoryService.ToggleCategoryFavoriteAsync(categoryId);
        await LoadAllAsync();
    }

    // --- Private async work methods ---

    private async Task NavigateMonthAsync(int monthDelta)
    {
        _view.DisplayedMonth = _view.DisplayedMonth.AddMonths(monthDelta);
        await LoadEventsForCurrentMonthAsync();
    }

    /// <summary>캘린더 그리드 + 즐겨찾기 목록 + 카테고리를 한 번에 갱신.</summary>
    private async Task LoadAllAsync()
    {
        await Task.WhenAll(
            LoadEventsForCurrentMonthAsync(),
            LoadFavoritesAsync()
        );
    }

    private async Task LoadEventsForCurrentMonthAsync()
    {
        // 현재 월 기준으로 앞뒤 1주일씩 여유 있게 이벤트를 불러와서 표시
        var start = _view.DisplayedMonth.AddDays(-7);
        var end = _view.DisplayedMonth.AddMonths(1).AddDays(7).AddTicks(-1);
        var events = await _eventService.GetEventsByDateRangeAsync(start, end);
        _view.EventsByDay = events
            .GroupBy(e => DateOnly.FromDateTime(e.StartDt))
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<Event>)g.OrderBy(e => e.StartDt).ToList());
    }

    private async Task LoadFavoritesAsync()
    {
        var evTask = _eventService.GetFavoritedEventsAsync();
        var catTask = _categoryService.GetAllCategoriesAsync();
        await Task.WhenAll(evTask, catTask);
        _view.Events = evTask.Result;
        _view.Categories = catTask.Result;
    }

    private async Task OpenEventDetailAsync(int eventId)
    {
        // TODO: retrieve event, construct EventDetailPresenter + view, show detail panel/dialog.
        await Task.CompletedTask;
    }

    private async Task SyncAndRefreshAsync()
    {
        if (_syncService.IsSyncing)
            return;

        await _syncService.SyncEventsAsync();
        await LoadAllAsync();
    }
}
