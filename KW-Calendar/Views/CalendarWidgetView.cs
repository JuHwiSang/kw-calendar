using System.ComponentModel;
using KW_Calendar.Models;
using KW_Calendar.Native;

namespace KW_Calendar.Views
{
    public partial class CalendarWidgetView : DesktopWidgetForm, ICalendarView
    {
        private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> _eventsByDay
            = new Dictionary<DateOnly, IReadOnlyList<Event>>();
        private IReadOnlyList<Event> _events = Array.Empty<Event>();
        private IReadOnlyList<Category> _categories = Array.Empty<Category>();

        public CalendarWidgetView()
        {
            InitializeComponent();
            RenderAll();
        }

        // ListBox 아이템으로 사용할 미니 DTO. 표시 텍스트 + 원본 id.
        private sealed record ListItem(int Id, string Display)
        {
            public override string ToString() => Display;
        }

        private void RenderAll()
        {
            RenderMonth();
            RenderEvents();
            RenderFavEvents();
            RenderCategories();
        }

        private void RenderMonth()
        {
            _monthLabel.Text = $"{_displayedMonth:yyyy년 M월}";
        }

        private void RenderEvents()
        {
            var start = DateOnly.FromDateTime(_displayedMonth);
            var end = DateOnly.FromDateTime(_displayedMonth.AddMonths(1).AddDays(-1));
            var flat = _eventsByDay
                .Where(kv => kv.Key >= start && kv.Key <= end)
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .Select(ev => new ListItem(
                    ev.Id,
                    $"{ev.StartDt:MM-dd HH:mm}  {ev.Title}"))
                .ToList();

            _eventsList.BeginUpdate();
            _eventsList.Items.Clear();
            if (flat.Count == 0)
            {
                _eventsList.Items.Add(new ListItem(-1, "(이벤트 없음)"));
            }
            else
            {
                foreach (var it in flat) _eventsList.Items.Add(it);
            }
            _eventsList.EndUpdate();
        }

        private void RenderFavEvents()
        {
            _favEventsList.BeginUpdate();
            _favEventsList.Items.Clear();
            if (_events.Count == 0)
            {
                _favEventsList.Items.Add(new ListItem(-1, "(없음)"));
            }
            else
            {
                foreach (var ev in _events)
                {
                    _favEventsList.Items.Add(new ListItem(ev.Id, $"★ {ev.Title}"));
                }
            }
            _favEventsList.EndUpdate();
        }

        private void RenderCategories()
        {
            _favCategoriesList.BeginUpdate();
            _favCategoriesList.Items.Clear();
            if (_categories.Count == 0)
            {
                _favCategoriesList.Items.Add(new ListItem(-1, "(없음)"));
            }
            else
            {
                foreach (var c in _categories)
                {
                    var star = c.IsFavorited ? "★" : "☆";
                    _favCategoriesList.Items.Add(new ListItem(c.Id, $"{star} {c.Name}"));
                }
            }
            _favCategoriesList.EndUpdate();
        }

        // --- 입력 핸들러 ---

        private void DragArea_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                BeginSystemDrag();
            }
        }

        private void EventsList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_eventsList.SelectedItem is ListItem { Id: > 0 } it)
            {
                EventSelected?.Invoke(this, it.Id);
                _eventsList.ClearSelected();
            }
        }

        private void FavEventsList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_favEventsList.SelectedItem is ListItem { Id: > 0 } it)
            {
                EventFavoriteToggleRequested?.Invoke(this, it.Id);
                _favEventsList.ClearSelected();
            }
        }

        private void FavCategoriesList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_favCategoriesList.SelectedItem is ListItem { Id: > 0 } it)
            {
                CategoryFavoriteToggleRequested?.Invoke(this, it.Id);
                _favCategoriesList.ClearSelected();
            }
        }

        // --- ICalendarView ---

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DateTime DisplayedMonth
        {
            get => _displayedMonth;
            set
            {
                _displayedMonth = value;
                RenderMonth();
                RenderEvents();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay
        {
            get => _eventsByDay;
            set
            {
                _eventsByDay = value ?? new Dictionary<DateOnly, IReadOnlyList<Event>>();
                RenderEvents();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new IReadOnlyList<Event> Events
        {
            get => _events;
            set
            {
                _events = value ?? Array.Empty<Event>();
                RenderFavEvents();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value ?? Array.Empty<Category>();
                RenderCategories();
            }
        }

        public event EventHandler? PreviousMonthRequested;
        public event EventHandler? NextMonthRequested;
        public event EventHandler<int>? EventSelected;
        public event EventHandler<int>? EventFavoriteToggleRequested;
        public event EventHandler<int>? CategoryFavoriteToggleRequested;
        public event EventHandler? SyncRequested;
    }
}
