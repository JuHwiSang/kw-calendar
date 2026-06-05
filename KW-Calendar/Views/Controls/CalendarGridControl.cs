using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KW_Calendar.Models;

namespace KW_Calendar.Views.Controls
{
    /// <summary>
    /// 월 헤더(이전/월/다음) + 요일 헤더 + 날짜 그리드 묶음.
    /// CalendarView / CalendarWidgetView가 공유한다.
    /// </summary>
    public partial class CalendarGridControl : UserControl
    {
        private static readonly Font FontMonthTitle = new("맑은 고딕", 16F, FontStyle.Bold);
        private static readonly Font FontDayLabel = new("맑은 고딕", 10F, FontStyle.Bold);
        private static readonly Font FontEventTag = new("맑은 고딕", 7.5F, FontStyle.Bold);
        private static readonly Font FontArrow = new("Arial", 28F, FontStyle.Regular);

        private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> _eventsByDay =
            new Dictionary<DateOnly, IReadOnlyList<Event>>();
        private IReadOnlyList<Category> _categories = new List<Category>();

        private const int CellPoolRows = 6;
        private const int CellPoolSize = CellPoolRows * 7;
        private readonly DayCell[] _cellPool = new DayCell[CellPoolSize];
        private int _currentRowCount = -1;

        public CalendarGridControl()
        {
            InitializeComponent();

            ApplyDesign();
            InitializeCellPool();
            EnableDoubleBuffered(panelHeader);

            btnPrev.Click += (s, e) => PreviousMonthRequested?.Invoke(this, EventArgs.Empty);
            btnNext.Click += (s, e) => NextMonthRequested?.Invoke(this, EventArgs.Empty);

            UpdateMonthTitle();
        }

        // --- 공개 API ---

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime DisplayedMonth
        {
            get => _displayedMonth;
            set
            {
                _displayedMonth = new DateTime(value.Year, value.Month, 1);
                if (!IsHandleCreated && tlpCalendar == null) return;
                UpdateMonthTitle();
                BuildCalendar(_displayedMonth.Year, _displayedMonth.Month);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay
        {
            get => _eventsByDay;
            set
            {
                _eventsByDay = value ?? new Dictionary<DateOnly, IReadOnlyList<Event>>();
                if (tlpCalendar != null)
                {
                    BuildCalendar(_displayedMonth.Year, _displayedMonth.Month);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value ?? new List<Category>();
                // 카테고리 즐겨찾기/색상이 이벤트 표시에 영향이 있으므로 다시 그린다.
                if (tlpCalendar != null)
                {
                    BuildCalendar(_displayedMonth.Year, _displayedMonth.Month);
                }
            }
        }

        public event EventHandler? PreviousMonthRequested;
        public event EventHandler? NextMonthRequested;
        public event EventHandler<int>? EventSelected;
        public event EventHandler<DateOnly>? DaySelected;

        // --- 디자인 적용 ---

        private void ApplyDesign()
        {
            BackColor = Color.White;

            lblMonthYear.Font = FontMonthTitle;
            lblMonthYear.ForeColor = Color.FromArgb(136, 19, 55);

            StyleArrowButton(btnPrev);
            StyleArrowButton(btnNext);

            Label[] dayLabels = { lblSun, lblMon, lblTue, lblWed, lblThu, lblFri, lblSat };
            foreach (Label label in dayLabels)
            {
                label.Font = FontDayLabel;
                label.ForeColor = Color.FromArgb(107, 114, 128);
                label.BackColor = Color.White;
                label.TextAlign = ContentAlignment.MiddleCenter;
            }
            lblSun.ForeColor = Color.FromArgb(239, 68, 68);
            lblSat.ForeColor = Color.FromArgb(59, 130, 246);
        }

        private static void StyleArrowButton(Label button)
        {
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(107, 114, 128);
            button.Font = FontArrow;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
        }

        private void UpdateMonthTitle()
        {
            lblMonthYear.Text = $"{_displayedMonth.Year}년 {_displayedMonth.Month}월";
        }

        // --- 그리드 구축 ---

        private static void EnableDoubleBuffered(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(control, true);
        }

        private void InitializeCellPool()
        {
            EnableDoubleBuffered(tlpCalendar);

            tlpCalendar.SuspendLayout();
            try
            {
                tlpCalendar.Controls.Clear();
                tlpCalendar.ColumnStyles.Clear();
                tlpCalendar.RowStyles.Clear();

                tlpCalendar.ColumnCount = 7;
                tlpCalendar.RowCount = CellPoolRows;

                for (int i = 0; i < 7; i++)
                {
                    tlpCalendar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 7F));
                }
                for (int i = 0; i < CellPoolRows; i++)
                {
                    tlpCalendar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / CellPoolRows));
                }

                for (int i = 0; i < CellPoolSize; i++)
                {
                    var dayCell = new DayCell(this);
                    _cellPool[i] = dayCell;
                    tlpCalendar.Controls.Add(dayCell.Panel, i % 7, i / 7);
                }
            }
            finally
            {
                tlpCalendar.ResumeLayout(false);
                tlpCalendar.PerformLayout();
            }
        }

        private const int WM_SETREDRAW = 0x000B;

        private void BuildCalendar(int year, int month)
        {
            SendMessage(tlpCalendar.Handle, WM_SETREDRAW, 0, 0);
            tlpCalendar.SuspendLayout();
            try
            {
                DateTime firstDay = new DateTime(year, month, 1);
                int startCol = (int)firstDay.DayOfWeek;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                int rowCount = (int)Math.Ceiling((startCol + daysInMonth) / 7.0);
                if (rowCount < 4) rowCount = 4;

                if (rowCount != _currentRowCount)
                {
                    tlpCalendar.RowStyles.Clear();
                    for (int i = 0; i < CellPoolRows; i++)
                    {
                        float pct = i < rowCount ? 100F / rowCount : 0F;
                        tlpCalendar.RowStyles.Add(new RowStyle(SizeType.Percent, pct));
                    }
                    _currentRowCount = rowCount;
                }

                DateTime today = DateTime.Today;
                int day = 1;

                for (int i = 0; i < CellPoolSize; i++)
                {
                    int row = i / 7;
                    int col = i % 7;
                    var dayCell = _cellPool[i];

                    if (row >= rowCount)
                    {
                        dayCell.SetEmpty();
                        continue;
                    }

                    if (row == 0 && col < startCol)
                    {
                        dayCell.SetEmpty();
                        continue;
                    }

                    if (day > daysInMonth)
                    {
                        dayCell.SetEmpty();
                        continue;
                    }

                    bool isToday =
                        today.Year == year &&
                        today.Month == month &&
                        today.Day == day;

                    DateOnly date = new DateOnly(year, month, day);
                    if (!_eventsByDay.TryGetValue(date, out var dayEvents))
                    {
                        dayEvents = Array.Empty<Event>();
                    }

                    dayCell.SetDay(date, isToday, dayEvents);

                    day++;
                }
            }
            finally
            {
                tlpCalendar.ResumeLayout(false);
                tlpCalendar.PerformLayout();
                SendMessage(tlpCalendar.Handle, WM_SETREDRAW, 1, 0);
                tlpCalendar.Invalidate(true);
            }
        }

        // --- 카테고리 색상 ---

        private Color GetCategoryBackColor(Category? category)
        {
            return category?.Name switch
            {
                "학사/수업" => Color.FromArgb(254, 242, 242),
                "행사" => Color.FromArgb(255, 247, 237),
                "장학금/등록금/지원금" => Color.FromArgb(254, 252, 232),
                "취업/창업/경력" => Color.FromArgb(240, 253, 244),
                "국제/교환/유학생" => Color.FromArgb(240, 253, 250),
                "비교과/자기계발" => Color.FromArgb(239, 246, 255),
                "생활/복지/시설" => Color.FromArgb(238, 242, 255),
                "봉사" => Color.FromArgb(250, 245, 255),
                _ => Color.FromArgb(243, 244, 246)
            };
        }

        private Color GetCategoryForeColor(Category? category)
        {
            return category?.Name switch
            {
                "학사/수업" => Color.FromArgb(185, 28, 28),
                "행사" => Color.FromArgb(194, 65, 12),
                "장학금/등록금/지원금" => Color.FromArgb(161, 98, 7),
                "취업/창업/경력" => Color.FromArgb(21, 128, 61),
                "국제/교환/유학생" => Color.FromArgb(15, 118, 110),
                "비교과/자기계발" => Color.FromArgb(29, 78, 216),
                "생활/복지/시설" => Color.FromArgb(67, 56, 202),
                "봉사" => Color.FromArgb(126, 34, 206),
                _ => Color.FromArgb(75, 85, 99)
            };
        }

        // --- 내부 클래스: DayCell / EventTag ---

        private sealed class DayCell
        {
            private static readonly Color EmptyBorder = Color.FromArgb(243, 244, 246);
            private static readonly Color DayLabelFore = Color.FromArgb(31, 41, 55);
            private static readonly Color TodayBadgeFill = Color.FromArgb(190, 24, 73);

            private const int MaxEventTags = 8;
            private const int HeaderHeight = 34;
            private const int TagHeight = 22;
            private const int TagGap = 3;
            private const int SideMargin = 6;

            private readonly CalendarGridControl _owner;
            public RoundedPanel Panel { get; }
            private readonly Label _dayLabel;
            private readonly RoundedPanel _badge;
            private readonly Label _badgeText;

            private readonly EventTag[] _tagPool = new EventTag[MaxEventTags];
            private DateOnly _date;

            public DayCell(CalendarGridControl owner)
            {
                _owner = owner;

                Panel = new RoundedPanel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(3),
                    BorderRadius = 8,
                    FillColor = Color.White,
                    BorderColor = EmptyBorder,
                    BorderSize = 1
                };

                _dayLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = HeaderHeight,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = DayLabelFore,
                    BackColor = Color.Transparent,
                    Font = FontDayLabel,
                    Visible = false
                };

                _badge = new RoundedPanel
                {
                    Size = new Size(32, 32),
                    BorderRadius = 16,
                    FillColor = TodayBadgeFill,
                    BorderSize = 0,
                    Visible = false
                };

                _badgeText = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = FontDayLabel
                };
                _badge.Controls.Add(_badgeText);

                Panel.Controls.Add(_dayLabel);
                Panel.Controls.Add(_badge);

                for (int i = 0; i < MaxEventTags; i++)
                {
                    var tag = new EventTag(owner);
                    _tagPool[i] = tag;
                    Panel.Controls.Add(tag.Panel);
                }

                Panel.Resize += (s, e) => LayoutChildren();
                Panel.Click += (s, e) =>
                {
                    if (_date != default) _owner.DaySelected?.Invoke(_owner, _date);
                };
            }

            private void LayoutChildren()
            {
                int w = Panel.Width;
                int h = Panel.Height;
                if (w <= 0 || h <= 0) return;

                if (_badge.Visible)
                {
                    _badge.Left = (w - _badge.Width) / 2;
                    _badge.Top = 4;
                }

                int tagWidth = Math.Max(0, w - SideMargin * 2);
                int top = HeaderHeight;
                int available = h - HeaderHeight - 4;
                int maxVisible = Math.Max(0, (available + TagGap) / (TagHeight + TagGap));

                for (int i = 0; i < MaxEventTags; i++)
                {
                    var tag = _tagPool[i];
                    if (i < maxVisible && tag.IsAssigned)
                    {
                        tag.Panel.SetBounds(SideMargin, top, tagWidth, TagHeight);
                        tag.Panel.Visible = true;
                        top += TagHeight + TagGap;
                    }
                    else
                    {
                        tag.Panel.Visible = false;
                    }
                }
            }

            public void SetEmpty()
            {
                _date = default;
                _dayLabel.Visible = false;
                _badge.Visible = false;
                for (int i = 0; i < MaxEventTags; i++)
                {
                    _tagPool[i].Clear();
                    _tagPool[i].Panel.Visible = false;
                }
            }

            public void SetDay(DateOnly date, bool isToday, IReadOnlyList<Event> dayEvents)
            {
                _date = date;
                string dayText = date.Day.ToString();

                if (isToday)
                {
                    _dayLabel.Visible = false;
                    if (_badgeText.Text != dayText) _badgeText.Text = dayText;
                    _badge.Visible = true;
                }
                else
                {
                    _badge.Visible = false;
                    if (_dayLabel.Text != dayText) _dayLabel.Text = dayText;
                    _dayLabel.Visible = true;
                }

                int n = Math.Min(dayEvents.Count, MaxEventTags);
                for (int i = 0; i < MaxEventTags; i++)
                {
                    if (i < n)
                    {
                        var ev = dayEvents[i];
                        var category = _owner._categories.FirstOrDefault(c => c.Id == ev.CategoryId);
                        bool isFav = ev.IsFavorited || (category != null && category.IsFavorited);
                        _tagPool[i].Assign(ev, category, isFav);
                    }
                    else
                    {
                        _tagPool[i].Clear();
                    }
                }

                LayoutChildren();
            }
        }

        private sealed class EventTag
        {
            private readonly CalendarGridControl _owner;
            public RoundedPanel Panel { get; }
            private readonly Label _label;
            private int _eventId = int.MinValue;

            public bool IsAssigned => _eventId != int.MinValue;

            public EventTag(CalendarGridControl owner)
            {
                _owner = owner;

                Panel = new RoundedPanel
                {
                    BorderRadius = 5,
                    BorderSize = 0,
                    Cursor = Cursors.Hand,
                    Visible = false
                };

                _label = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 3, 6, 0),
                    BackColor = Color.Transparent,
                    Font = FontEventTag,
                    Cursor = Cursors.Hand,
                    AutoEllipsis = true
                };
                Panel.Controls.Add(_label);

                Panel.Click += OnClick;
                _label.Click += OnClick;
            }

            private void OnClick(object? sender, EventArgs e)
            {
                if (_eventId != int.MinValue)
                    _owner.EventSelected?.Invoke(_owner, _eventId);
            }

            public void Assign(Event ev, Category? category, bool isFavorite)
            {
                string title = isFavorite ? "★ " + ev.Title : ev.Title;
                if (_label.Text != title) _label.Text = title;
                _label.ForeColor = _owner.GetCategoryForeColor(category);
                Panel.FillColor = _owner.GetCategoryBackColor(category);
                _eventId = ev.Id;
            }

            public void Clear()
            {
                _eventId = int.MinValue;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
