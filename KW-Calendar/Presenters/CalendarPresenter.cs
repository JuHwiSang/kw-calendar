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
        _view.AddEventRequested += OnAddEventRequested;
        _view.EventSelected += OnEventSelected;
        // _view.ShowFavoritesOnlyChanged += OnShowFavoritesOnlyChanged;  // TODO: 즐겨찾기만 보기 필터 (우선순위 낮음)
        _view.SyncRequested += OnSyncRequested;
        _view.EventFavoriteToggleRequested += OnEventFavoriteToggleRequested;
        _view.CategoryFavoriteToggleRequested += OnCategoryFavoriteToggleRequested;

        _view.DisplayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _ = SyncAndRefreshAsync();
    }

    // --- 캘린더 내비게이션 ---

    private async void OnPreviousMonthRequested(object? sender, EventArgs e)
        => await NavigateMonthAsync(-1);

    private async void OnNextMonthRequested(object? sender, EventArgs e)
        => await NavigateMonthAsync(1);

    private async void OnAddEventRequested(object? sender, DateOnly date)
    {
        try
        {
            var addView = new AddEventView();
            var addPresenter = new AddEventPresenter(addView, _eventService, _categoryService);
            await addPresenter.InitializeAsync(date);
            addView.ShowDialog();
            await LoadAllAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"일정 추가 창을 여는 중 오류가 발생했습니다.\n{ex.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnEventSelected(object? sender, int eventId)
        => await OpenEventDetailAsync(eventId);

    // TODO: 즐겨찾기만 보기 필터 (우선순위 낮음)
    // private async void OnShowFavoritesOnlyChanged(object? sender, bool showFavoritesOnly)
    // {
    //     _showFavoritesOnly = showFavoritesOnly;
    //     await LoadEventsForCurrentMonthAsync();
    // }


    private async void OnSyncRequested(object? sender, EventArgs e)
    {
        try
        {
            await SyncAndRefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"동기화 중 오류가 발생했습니다.\n{ex.Message}",
                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

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



    private async Task SyncAndRefreshAsync()
    {
        if (_syncService.IsSyncing)
            return;

        await _syncService.SyncEventsAsync();
        await LoadAllAsync();
    }
    /*private async Task OpenEventDetailAsync(int eventId)
    {
        var detailView = new EventDetailView();
        var detailPresenter = new EventDetailPresenter(detailView, _eventService);

        //detailPresenter.Initialize(eventId);
        await detailPresenter.InitializeAsync(eventId); //EventdetailPresenter.cs 수정된 메서드와 같이 수정

        if (_view is System.Windows.Forms.Form owner)
        {
            detailView.ShowDialog(owner);
        }
        else
        {
            detailView.ShowDialog();
        }

        await LoadAllAsync();
    }*/
    private Task OpenEventDetailAsync(int eventId)
    {
        var detailView = new EventDetailView();
        var detailPresenter = new EventDetailPresenter(detailView, _eventService);

        detailPresenter.Initialize(eventId);

        detailPresenter.FavoriteToggled += async (s, ev) => await LoadAllAsync();
        detailPresenter.DeleteRequested += async (s, id) => await LoadAllAsync();
        detailPresenter.EditRequested += async (s, id) =>
        {
            try
            {
                var ev = await _eventService.GetEventByIdAsync(id);
                if (ev == null) return;
                var editView = new AddEventView();
                var editPresenter = new EditEventPresenter(editView, _eventService, _categoryService, ev);
                await editPresenter.InitializeAsync();
                editView.ShowDialog();
                if (editPresenter.WasSaved)
                    await detailPresenter.ReloadAsync();
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"일정 편집 창을 여는 중 오류가 발생했습니다.\n{ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        detailView.FormClosed += async (s, e) => await LoadAllAsync();

        // owner를 지정하면 detail이 항상 owner 위에 떠 owner 클릭 시 앞으로 못 옴.
        // 모달리스 + 독립 z-order를 원하므로 owner 없이 Show.
        detailView.Show();

        return Task.CompletedTask;
    }


    //추가
    /*private async Task SyncAndRefreshAsync()
    {
        if (_syncService.IsSyncing)
            return;

        await _syncService.SyncEventsAsync();
        await LoadAllAsync();
    }*/
}
