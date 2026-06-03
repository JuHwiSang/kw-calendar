using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KW_Calendar.Models;
using KW_Calendar.Native;
using System.ComponentModel;//추가

namespace KW_Calendar.Views
{
    public partial class CalendarView : Form, ICalendarView
    {
        private const int FixedYear = 2026;

        private static readonly Font FontMonthTitle = new("맑은 고딕", 16F, FontStyle.Bold);
        private static readonly Font FontCategoryTitle = new("맑은 고딕", 11F, FontStyle.Bold);
        private static readonly Font FontDayLabel = new("맑은 고딕", 10F, FontStyle.Bold);
        private static readonly Font FontRadio = new("맑은 고딕", 9.5F, FontStyle.Bold);
        private static readonly Font FontEmptyLabel = new("맑은 고딕", 9F, FontStyle.Bold);
        private static readonly Font FontEventTag = new("맑은 고딕", 7.5F, FontStyle.Bold);
        private static readonly Font FontArrow = new("Arial", 28F, FontStyle.Regular);
        private static readonly Font FontStar = new("Segoe UI Symbol", 13F, FontStyle.Regular);

        private int currentYear = FixedYear;
        private int currentMonth = DateTime.Today.Year == FixedYear ? DateTime.Today.Month : 1;

        //상세 패널 분리(삭제)
        //private int detailScrollY = 0;
        //private const int DetailScrollStep = 45;

        private DateTime displayedMonth;

        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> eventsByDay =
            new Dictionary<DateOnly, IReadOnlyList<Event>>();

        private IReadOnlyList<Event> events =
            new List<Event>();

        private IReadOnlyList<Category> modelCategories =
            new List<Category>();

        private const int CellPoolRows = 6;
        private const int CellPoolSize = CellPoolRows * 7;
        private readonly DayCell[] cellPool = new DayCell[CellPoolSize];
        private int currentRowCount = -1;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime DisplayedMonth
        {
            get => displayedMonth;
            set
            {
                displayedMonth = value;

                currentYear = value.Year;
                currentMonth = value.Month;

                if (lblMonthYear != null && tlpCalendar != null)
                {
                    UpdateMonthTitle();
                    BuildCalendar(currentYear, currentMonth);
                    UpdateArrowState();
                }
            }
        }//대체 코드
        /*public DateTime DisplayedMonth
        {
            get => displayedMonth;
            set
            {
                displayedMonth = value;

                currentYear = value.Year;
                currentMonth = value.Month;

                if (lblMonthYear != null && tlpCalendar != null)
                {
                    UpdateMonthTitle();
                    BuildCalendar(currentYear, currentMonth);
                    UpdateArrowState();
                }
            }

        }*/

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay
        {
            get => eventsByDay;
            set
            {
                eventsByDay = value ?? new Dictionary<DateOnly, IReadOnlyList<Event>>();

                if (tlpCalendar != null)
                {
                    BuildCalendar(currentYear, currentMonth);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Event> Events
        {
            get => events;
            set
            {
                events = value ?? new List<Event>();
            }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Category> Categories
        {
            get => modelCategories;
            set
            {
                modelCategories = value ?? new List<Category>();

                if (flpCategories != null)
                {
                    BuildCategoryList();
                }
                // 카테고리 IsFavorited는 이벤트 별표 판정에도 쓰이므로 캘린더도 다시 그린다.
                if (tlpCalendar != null)
                {
                    BuildCalendar(currentYear, currentMonth);
                }
            }
        }

        public event EventHandler PreviousMonthRequested;
        public event EventHandler NextMonthRequested;
        public event EventHandler SyncRequested;
        public event EventHandler<int> EventSelected;
        public event EventHandler<int> EventFavoriteToggleRequested;
        public event EventHandler<int> CategoryFavoriteToggleRequested;

        //Sample데이터 삭제
        //대신 Present 데이터 가져오기
        

        public CalendarView()
        {
            InitializeComponent();

            displayedMonth = new DateTime(currentYear, currentMonth, 1);

            ApplyCalendarDesign();
            InitializeCellPool();
            InitializeTitleBar();
            EnableDoubleBuffered(panelHeader);

            rbAll.CheckedChanged += FilterRadio_CheckedChanged;
            rbFav.CheckedChanged += FilterRadio_CheckedChanged;

            btnPrev.Click += BtnPrev_Click;
            btnNext.Click += BtnNext_Click;

            //삭제
            /*lblDetailClose.Click += (s, e) => HideEventDetail();

            lblScrollUp.Click += (s, e) => ScrollDetail(-DetailScrollStep);
            lblScrollDown.Click += (s, e) => ScrollDetail(DetailScrollStep);

            detailBody.MouseWheel += DetailBody_MouseWheel;
            detailContent.MouseWheel += DetailBody_MouseWheel;

            AttachDetailMouseWheel(detailContent);
            UpdateDetailThumb();*/

            lblDetailClose.MouseEnter += (s, e) =>
            {
                lblDetailClose.ForeColor = Color.FromArgb(190, 24, 73);
            };

            lblDetailClose.MouseLeave += (s, e) =>
            {
                lblDetailClose.ForeColor = Color.FromArgb(107, 114, 128);
            };

            /*Presenter가 초기화 담당*/
            //BuildCalendar(currentYear, currentMonth);
            //BuildCategoryList();
            //UpdateArrowState();
        }

        private void ApplyCalendarDesign()
        {
            BackColor = Color.FromArgb(238, 239, 241);

            sideArea.BackColor = Color.FromArgb(249, 250, 251);

            lblMonthYear.Font = FontMonthTitle;
            lblMonthYear.ForeColor = Color.FromArgb(136, 19, 55);

            btnPrev.Text = "‹";
            btnNext.Text = "›";

            StyleArrowButton(btnPrev);
            StyleArrowButton(btnNext);

            Label[] dayLabels =
            {
                lblSun,
                lblMon,
                lblTue,
                lblWed,
                lblThu,
                lblFri,
                lblSat
            };

            foreach (Label label in dayLabels)
            {
                label.Font = FontDayLabel;
                label.ForeColor = Color.FromArgb(107, 114, 128);
                label.BackColor = Color.White;
                label.TextAlign = ContentAlignment.MiddleCenter;
            }

            lblSun.ForeColor = Color.FromArgb(239, 68, 68);
            lblSat.ForeColor = Color.FromArgb(59, 130, 246);

            StyleRadioButton(rbFav);
            StyleRadioButton(rbAll);

            lblCategoryTitle.Font = FontCategoryTitle;
            lblCategoryTitle.ForeColor = Color.FromArgb(31, 41, 55);

            UpdateMonthTitle();
        }

        private void StyleArrowButton(Label button)
        {
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(107, 114, 128);
            button.Font = FontArrow;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
        }

        private void StyleRadioButton(RadioButton radioButton)
        {
            radioButton.Font = FontRadio;
            radioButton.ForeColor = Color.FromArgb(31, 41, 55);
            radioButton.BackColor = Color.FromArgb(249, 250, 251);
            radioButton.Cursor = Cursors.Hand;
        }

        private void FilterRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAll.Checked || rbFav.Checked)
            {
                BuildCategoryList();
            }
        }

        /*private void BtnPrev_Click(object sender, EventArgs e)
        {
            PreviousMonthRequested?.Invoke(this, EventArgs.Empty);

            if (currentMonth <= 1)
                return;

            currentMonth--;
            currentYear = FixedYear;
            displayedMonth = new DateTime(currentYear, currentMonth, 1);

            UpdateMonthTitle();
            BuildCalendar(currentYear, currentMonth);
            UpdateArrowState();
        }

        private void BtnNext_Click(object sender, EventArgs e)*/
        //Presenter에서 조작
        private void BtnNext_Click(object sender, EventArgs e)
        {
            NextMonthRequested?.Invoke(this, EventArgs.Empty);
        }
        private void BtnPrev_Click(object sender, EventArgs e)
        {
            PreviousMonthRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateMonthTitle()
        {
            //currentYear = FixedYear; /*위 생성자 수정. Presenter 담당*/
            lblMonthYear.Text = $"{currentYear}년 {currentMonth}월";
        }

        // TODO: 화살표가 disabled되는 케이스가 영영 없다면 이 메서드와 호출부 3곳을
        // 제거하고 StyleArrowButton에서 한 번만 세팅하도록 정리.
        private void UpdateArrowState()
        {
            btnPrev.ForeColor = Color.FromArgb(107, 114, 128);
            btnNext.ForeColor = Color.FromArgb(107, 114, 128);
            btnPrev.Cursor = Cursors.Hand;
            btnNext.Cursor = Cursors.Hand;
        }

        private const int ResizeBorder = 6;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowHelpers.ApplyRoundedCorners(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WindowHelpers.WM_NCHITTEST)
            {
                int x = (short)(m.LParam.ToInt32() & 0xFFFF);
                int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);
                var pt = PointToClient(new Point(x, y));
                var hit = WindowHelpers.GetResizeHitCode(this, pt, ResizeBorder);
                if (hit.HasValue)
                {
                    m.Result = (IntPtr)hit.Value;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void InitializeTitleBar()
        {
            var titleBar = new CustomTitleBar
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White,
                TitleText = "KW Calendar"
            };
            Controls.Add(titleBar);
            titleBar.BringToFront();
        }

        private static void EnableDoubleBuffered(Control control)
        {
            // TableLayoutPanel은 DoubleBuffered가 protected라 리플렉션으로 켠다.
            // 셀 단위 페인트가 누적돼 "드르륵" 그려지는 현상을 막기 위함.
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
                    cellPool[i] = dayCell;
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
                if (rowCount < 4)
                    rowCount = 4;

                if (rowCount != currentRowCount)
                {
                    tlpCalendar.RowStyles.Clear();
                    for (int i = 0; i < CellPoolRows; i++)
                    {
                        float pct = i < rowCount ? 100F / rowCount : 0F;
                        tlpCalendar.RowStyles.Add(new RowStyle(SizeType.Percent, pct));
                    }
                    currentRowCount = rowCount;
                }

                DateTime today = DateTime.Today;
                int day = 1;

                for (int i = 0; i < CellPoolSize; i++)
                {
                    int row = i / 7;
                    int col = i % 7;
                    var dayCell = cellPool[i];

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
                        today.Year == FixedYear &&
                        today.Month == month &&
                        today.Day == day;

                    DateOnly date = new DateOnly(year, month, day);
                    if (!eventsByDay.TryGetValue(date, out var dayEvents))
                    {
                        dayEvents = Array.Empty<Event>();
                    }

                    dayCell.SetDay(day, isToday, dayEvents);

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

        private sealed class DayCell
        {
            private static readonly Color EmptyBorder = Color.FromArgb(243, 244, 246);
            private static readonly Color DayLabelFore = Color.FromArgb(31, 41, 55);
            private static readonly Color TodayBadgeFill = Color.FromArgb(190, 24, 73);

            private const int MaxEventTags = 8;
            private const int HeaderHeight = 34;   // 날짜 라벨/배지 영역 높이
            private const int TagHeight = 18;
            private const int TagGap = 3;
            private const int SideMargin = 6;

            private readonly CalendarView _owner;
            public RoundedPanel Panel { get; }
            private readonly Label _dayLabel;
            private readonly RoundedPanel _badge;
            private readonly Label _badgeText;

            private readonly EventTag[] _tagPool = new EventTag[MaxEventTags];

            public DayCell(CalendarView owner)
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
            }

            // TODO: 셀 높이가 부족해 표시 못한 이벤트가 있으면 마지막 칸을 "+N more"로 대체.
            // 현재는 그냥 잘려서 숨겨짐. 구현 시: maxVisible로 잘리는 시점에 잔여 개수를 계산해
            // 마지막 태그를 "+N" 표시로 바꾸고 클릭 시 (예: 그 날짜 상세 패널 열기) 처리.
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
                int available = h - HeaderHeight - 4; // 아래 4px 여백
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
                _dayLabel.Visible = false;
                _badge.Visible = false;
                for (int i = 0; i < MaxEventTags; i++)
                {
                    _tagPool[i].Clear();
                    _tagPool[i].Panel.Visible = false;
                }
            }

            public void SetDay(int day, bool isToday, IReadOnlyList<Event> dayEvents)
            {
                string dayText = day.ToString();

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
                        var category = _owner.modelCategories.FirstOrDefault(c => c.Id == ev.CategoryId);
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
            private readonly CalendarView _owner;
            public RoundedPanel Panel { get; }
            private readonly Label _label;
            private int _eventId = -1;

            public bool IsAssigned => _eventId >= 0;

            public EventTag(CalendarView owner)
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
                    Padding = new Padding(6, 0, 6, 0),
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
                if (_eventId >= 0)
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
                _eventId = -1;
            }
        }
        //삭제
       /* private void OpenEventDetail(CalendarEventInfo eventInfo)
        {
            EventSelected?.Invoke(this, eventInfo.EventId);
            ShowEventDetail(eventInfo);
        }*/

        private void BuildCategoryList()
        {
            SendMessage(flpCategories.Handle, WM_SETREDRAW, 0, 0);
            flpCategories.SuspendLayout();
            try
            {
                flpCategories.Controls.Clear();

                int visibleCount = 0;

                //수정
                foreach (Category category in modelCategories)
                {
                    if (rbFav.Checked && !category.IsFavorited)
                        continue;

                    AddCategoryItem(category);
                    visibleCount++;
                }
                //

                if (visibleCount == 0)
                {
                    Label emptyLabel = new Label
                    {
                        Text = "즐겨찾기 일정이 없습니다.",
                        Width = 198,
                        Height = 40,
                        ForeColor = Color.FromArgb(156, 163, 175),
                        Font = FontEmptyLabel,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    flpCategories.Controls.Add(emptyLabel);
                }
            }
            finally
            {
                flpCategories.ResumeLayout(false);
                flpCategories.PerformLayout();
                SendMessage(flpCategories.Handle, WM_SETREDRAW, 1, 0);
                flpCategories.Invalidate(true);
            }
        }


        //수정
        private void AddCategoryItem(Category category)
        {
            RoundedPanel item = new RoundedPanel
            {
                Width = 198,
                Height = 40,
                Margin = new Padding(0, 0, 0, 10),
                BorderRadius = 8,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(229, 231, 235),
                BorderSize = 1
            };

            RoundedPanel dot = new RoundedPanel
            {
                Location = new Point(16, 16),
                Size = new Size(13, 13),
                BorderRadius = 5,
                BorderSize = 0,
                FillColor = GetCategoryDotColor(category),
                BackColor = Color.Transparent
            };

            Label text = new Label
            {
                Text = category.Name,
                ForeColor = Color.FromArgb(31, 41, 55),
                BackColor = Color.Transparent,
                Font = FontRadio,
                Location = new Point(42, 9),
                Size = new Size(122, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label starButton = CreateStarButton(category);

            item.Controls.Add(dot);
            item.Controls.Add(text);
            item.Controls.Add(starButton);

            flpCategories.Controls.Add(item);
        }


        //수정
        private Label CreateStarButton(Category category)
        {
            bool canToggle = rbAll.Checked;

            Label starButton = new Label
            {
                Location = new Point(170, 5),
                Size = new Size(22, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Font = FontStar,
                Cursor = canToggle ? Cursors.Hand : Cursors.Default
            };

            if (category.IsFavorited)
            {
                starButton.Text = "★";
                starButton.ForeColor = Color.FromArgb(250, 204, 21);
            }
            else
            {
                starButton.Text = "☆";
                starButton.ForeColor = Color.FromArgb(203, 213, 225);
            }

            if (canToggle)
            {
                starButton.Click += (s, e) =>
                {
                    CategoryFavoriteToggleRequested?.Invoke(this, category.Id);
                };
            }

            return starButton;
        }

        //카테고리 색상 보조 메서드 추가(이름 기준으로 색상 추가)
        private Color GetCategoryDotColor(Category? category)
        {
            return category?.Name switch
            {
                "학사/수업" => Color.FromArgb(248, 113, 113),
                "행사" => Color.FromArgb(251, 146, 60),
                "장학금/등록금/지원금" => Color.FromArgb(250, 204, 21),
                "취업/창업/경력" => Color.FromArgb(74, 222, 128),
                "국제/교환/유학생" => Color.FromArgb(45, 212, 191),
                "비교과/자기계발" => Color.FromArgb(96, 165, 250),
                "생활/복지/시설" => Color.FromArgb(129, 140, 248),
                "봉사" => Color.FromArgb(192, 132, 252),
                _ => Color.FromArgb(156, 163, 175)
            };
        }

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

        //수정 -> 삭제
        /*private void ToggleFavorite(CategoryInfo category)
        {
            if (!rbAll.Checked)
                return;

            category.IsFavorite = !category.IsFavorite;

            int categoryId = categories.IndexOf(category);

            if (categoryId >= 0)
            {
                CategoryFavoriteToggleRequested?.Invoke(this, categoryId);
            }

            BuildCategoryList();
            BuildCalendar(currentYear, currentMonth);
        }*/

        //ShowEventDetail함수 삭제
        /*private void ShowEventDetail(CalendarEventInfo eventInfo)
        {
            CategoryInfo linkedCategory = categories.Find(c => c.Title == eventInfo.CategoryTitle);

            lblDetailTitle.Text = eventInfo.DetailTitle;
            lblDetailTimeValue.Text = eventInfo.TimeText;
            lblDetailPlaceValue.Text = eventInfo.PlaceText;
            lblDetailOriginalValue.Text = eventInfo.OriginalText;

            detailHeader.BackColor = linkedCategory != null
                ? linkedCategory.EventBackColor
                : Color.FromArgb(254, 226, 226);

            lblDetailStar.Visible = linkedCategory != null && linkedCategory.IsFavorite;

            detailPanel.Visible = true;
            detailPanel.BringToFront();

            detailHeader.BringToFront();
            lblDetailClose.BringToFront();

            detailScrollY = 0;
            detailContent.Top = 0;
            UpdateDetailThumb();
        }*/

        //메서드 전부 삭제
        /*private void HideEventDetail()

        private void DetailBody_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                ScrollDetail(DetailScrollStep);
            else
                ScrollDetail(-DetailScrollStep);
        }

        private void ScrollDetail(int delta)
        {
            int maxScroll = Math.Max(0, detailContent.Height - detailBody.Height);

            detailScrollY += delta;

            if (detailScrollY < 0)
                detailScrollY = 0;

            if (detailScrollY > maxScroll)
                detailScrollY = maxScroll;

            detailContent.Top = -detailScrollY;

            UpdateDetailThumb();
        }

        private void UpdateDetailThumb()
        {
            int maxScroll = Math.Max(0, detailContent.Height - detailBody.Height);

            int areaTop = 34;
            int areaBottom = detailScrollTrack.Height - 34;
            int movableHeight = areaBottom - areaTop - detailScrollThumb.Height;

            if (maxScroll <= 0)
            {
                detailScrollThumb.Top = areaTop;
                detailScrollThumb.Visible = false;
                return;
            }

            detailScrollThumb.Visible = true;
            detailScrollThumb.Top = areaTop + (detailScrollY * movableHeight / maxScroll);
        }

        private void AttachDetailMouseWheel(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.MouseWheel += DetailBody_MouseWheel;

                if (control.HasChildren)
                    AttachDetailMouseWheel(control);
            }
        }*/

        //CalendarEventInfo, CategoryInfo 클래스도 삭제

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
