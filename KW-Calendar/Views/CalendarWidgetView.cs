using System.ComponentModel;
using KW_Calendar.Models;
using KW_Calendar.Native;

namespace KW_Calendar.Views
{
    public partial class CalendarWidgetView : DesktopWidgetForm, ICalendarView
    {
        // 화면 우측 상단에서 떨어뜨릴 여백.
        private const int InitialMargin = 24;

        public CalendarWidgetView()
        {
            InitializeComponent();

            _calendarGrid.PreviousMonthRequested += (s, e) => PreviousMonthRequested?.Invoke(this, e);
            _calendarGrid.NextMonthRequested += (s, e) => NextMonthRequested?.Invoke(this, e);
            _calendarGrid.EventSelected += (s, id) => EventSelected?.Invoke(this, id);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 기본 위치: 메인 화면 우측 상단에서 margin만큼 떨어진 곳.
            var screen = Screen.FromControl(this) ?? Screen.PrimaryScreen;
            if (screen != null)
            {
                var area = screen.WorkingArea;
                Location = new Point(
                    area.Right - Width - InitialMargin,
                    area.Top + InitialMargin);
            }
        }

        // --- ICalendarView ---

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DateTime DisplayedMonth
        {
            get => _calendarGrid.DisplayedMonth;
            set => _calendarGrid.DisplayedMonth = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay
        {
            get => _calendarGrid.EventsByDay;
            set => _calendarGrid.EventsByDay = value;
        }

        // 위젯에는 즐겨찾기 패널이 없지만, 인터페이스 충족을 위해 보관만 한다.
        private IReadOnlyList<Event> _events = Array.Empty<Event>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new IReadOnlyList<Event> Events
        {
            get => _events;
            set => _events = value ?? Array.Empty<Event>();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<Category> Categories
        {
            get => _calendarGrid.Categories;
            set => _calendarGrid.Categories = value;
        }

        public event EventHandler? PreviousMonthRequested;
        public event EventHandler? NextMonthRequested;
        public event EventHandler<int>? EventSelected;
        public event EventHandler<int>? EventFavoriteToggleRequested;
        public event EventHandler<int>? CategoryFavoriteToggleRequested;
        public event EventHandler? SyncRequested;
    }
}
