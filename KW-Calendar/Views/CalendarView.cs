using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KW_Calendar.Models;

namespace KW_Calendar.Views
{
    public partial class CalendarView : Form, ICalendarView
    {
        private const int FixedYear = 2026;

        private int currentYear = FixedYear;
        private int currentMonth = DateTime.Today.Year == FixedYear ? DateTime.Today.Month : 1;

        private int detailScrollY = 0;
        private const int DetailScrollStep = 45;

        private DateTime displayedMonth;

        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> eventsByDay =
            new Dictionary<DateOnly, IReadOnlyList<Event>>();

        private IReadOnlyList<Event> events =
            new List<Event>();

        private IReadOnlyList<Category> modelCategories =
            new List<Category>();

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
        }

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

        public IReadOnlyList<Event> Events
        {
            get => events;
            set
            {
                events = value ?? new List<Event>();
            }
        }

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
            }
        }

        public event EventHandler PreviousMonthRequested;
        public event EventHandler NextMonthRequested;
        public event EventHandler<int> EventSelected;
        public event EventHandler<int> EventFavoriteToggleRequested;
        public event EventHandler<int> CategoryFavoriteToggleRequested;

        private readonly Dictionary<int, CalendarEventInfo> sampleEvents =
            new Dictionary<int, CalendarEventInfo>
            {
                {
                    2,
                    new CalendarEventInfo(
                        2,
                        "근로...",
                        "근로 장학 신청",
                        "장학금/등록금/지원금",
                        "09:00 - 18:00",
                        "온라인 신청",
                        "2026학년도 근로 장학 신청 안내입니다.\n신청 기간 내 포털을 통해 접수해 주시기 바랍니다.\n세부 일정 및 제출 서류는 공지사항을 확인해 주세요."
                    )
                },
                {
                    10,
                    new CalendarEventInfo(
                        10,
                        "SW학...",
                        "SW 학습 프로그램",
                        "비교과/자기계발",
                        "13:00 - 15:00",
                        "새빛관 강의실",
                        "SW 학습 역량 강화를 위한 비교과 프로그램입니다.\n참여를 원하는 학생은 사전 신청 후 참석해 주세요."
                    )
                },
                {
                    14,
                    new CalendarEventInfo(
                        14,
                        "알고...",
                        "알고리즘 팀플",
                        "학사/수업",
                        "14:00 - 16:00",
                        "소프트웨어관 301호",
                        "안녕하세요, 알고리즘 2분반 조교입니다.\n기말 프로젝트 중간 점검 및 역할 분담을 위한 팀별 대면 미팅을 진행합니다.\n팀장들은 각 팀원들의 개발 진행 상황을 사전에 취합하여 제출해주시기 바라며, 미팅 시 조별 노트북을 최소 1대 이상 필히 지참해주시기 바랍니다.\n불참 시 팀 점수에 불이익이 있을 수 있습니다."
                    )
                },
                {
                    15,
                    new CalendarEventInfo(
                        15,
                        "공운...",
                        "공운 행사",
                        "행사",
                        "11:00 - 12:00",
                        "교내 광장",
                        "교내 행사 안내입니다.\n참여 학생은 행사 시작 10분 전까지 도착해 주세요."
                    )
                },
                {
                    16,
                    new CalendarEventInfo(
                        16,
                        "광운...",
                        "광운 공지",
                        "행사",
                        "10:00 - 11:00",
                        "온라인",
                        "광운대학교 공지사항입니다.\n자세한 내용은 원문 페이지를 확인해 주세요."
                    )
                },
                {
                    17,
                    new CalendarEventInfo(
                        17,
                        "광운...",
                        "광운 안내",
                        "행사",
                        "15:00 - 16:00",
                        "복지관",
                        "광운대학교 행사 안내입니다.\n세부 일정은 추후 변경될 수 있습니다."
                    )
                },
                {
                    28,
                    new CalendarEventInfo(
                        28,
                        "기말...",
                        "기말고사",
                        "학사/수업",
                        "09:00 - 11:00",
                        "강의실 추후 공지",
                        "기말고사 일정 안내입니다.\n시험 범위와 준비물은 수업 공지를 확인해 주세요."
                    )
                }
            };

        private readonly List<CategoryInfo> categories =
            new List<CategoryInfo>
            {
                new CategoryInfo(
                    "학사/수업",
                    Color.FromArgb(248, 113, 113),
                    Color.FromArgb(254, 242, 242),
                    Color.FromArgb(185, 28, 28),
                    true
                ),

                new CategoryInfo(
                    "행사",
                    Color.FromArgb(251, 146, 60),
                    Color.FromArgb(255, 247, 237),
                    Color.FromArgb(194, 65, 12),
                    true
                ),

                new CategoryInfo(
                    "장학금/등록금/지원금",
                    Color.FromArgb(250, 204, 21),
                    Color.FromArgb(254, 252, 232),
                    Color.FromArgb(161, 98, 7),
                    true
                ),

                new CategoryInfo(
                    "취업/창업/경력",
                    Color.FromArgb(74, 222, 128),
                    Color.FromArgb(240, 253, 244),
                    Color.FromArgb(21, 128, 61),
                    true
                ),

                new CategoryInfo(
                    "국제/교환/유학생",
                    Color.FromArgb(45, 212, 191),
                    Color.FromArgb(240, 253, 250),
                    Color.FromArgb(15, 118, 110),
                    true
                ),

                new CategoryInfo(
                    "비교과/자기계발",
                    Color.FromArgb(96, 165, 250),
                    Color.FromArgb(239, 246, 255),
                    Color.FromArgb(29, 78, 216),
                    true
                ),

                new CategoryInfo(
                    "생활/복지/시설",
                    Color.FromArgb(129, 140, 248),
                    Color.FromArgb(238, 242, 255),
                    Color.FromArgb(67, 56, 202),
                    true
                ),

                new CategoryInfo(
                    "봉사",
                    Color.FromArgb(192, 132, 252),
                    Color.FromArgb(250, 245, 255),
                    Color.FromArgb(126, 34, 206),
                    true
                )
            };

        public CalendarView()
        {
            InitializeComponent();

            displayedMonth = new DateTime(currentYear, currentMonth, 1);

            ApplyCalendarDesign();

            rbAll.CheckedChanged += FilterRadio_CheckedChanged;
            rbFav.CheckedChanged += FilterRadio_CheckedChanged;

            btnPrev.Click += BtnPrev_Click;
            btnNext.Click += BtnNext_Click;

            lblDetailClose.Click += (s, e) => HideEventDetail();

            lblScrollUp.Click += (s, e) => ScrollDetail(-DetailScrollStep);
            lblScrollDown.Click += (s, e) => ScrollDetail(DetailScrollStep);

            detailBody.MouseWheel += DetailBody_MouseWheel;
            detailContent.MouseWheel += DetailBody_MouseWheel;

            AttachDetailMouseWheel(detailContent);
            UpdateDetailThumb();

            lblDetailClose.MouseEnter += (s, e) =>
            {
                lblDetailClose.ForeColor = Color.FromArgb(190, 24, 73);
            };

            lblDetailClose.MouseLeave += (s, e) =>
            {
                lblDetailClose.ForeColor = Color.FromArgb(107, 114, 128);
            };

            BuildCalendar(currentYear, currentMonth);
            BuildCategoryList();
            UpdateArrowState();

            EnableFormDrag(mainCard);
            EnableFormDrag(leftArea);
            EnableFormDrag(sideArea);
            EnableFormDrag(panelHeader);
        }

        private void ApplyCalendarDesign()
        {
            BackColor = Color.FromArgb(238, 239, 241);

            mainCard.FillColor = Color.White;
            mainCard.BorderSize = 0;
            mainCard.BorderRadius = 28;

            sideArea.BackColor = Color.FromArgb(249, 250, 251);

            lblMonthYear.Font = new Font("맑은 고딕", 16F, FontStyle.Bold);
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
                label.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
                label.ForeColor = Color.FromArgb(107, 114, 128);
                label.BackColor = Color.White;
                label.TextAlign = ContentAlignment.MiddleCenter;
            }

            lblSun.ForeColor = Color.FromArgb(239, 68, 68);
            lblSat.ForeColor = Color.FromArgb(59, 130, 246);

            StyleRadioButton(rbFav);
            StyleRadioButton(rbAll);

            lblCategoryTitle.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblCategoryTitle.ForeColor = Color.FromArgb(31, 41, 55);

            UpdateMonthTitle();
        }

        private void StyleArrowButton(Label button)
        {
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(107, 114, 128);
            button.Font = new Font("Arial", 28F, FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Cursor = Cursors.Hand;
        }

        private void StyleRadioButton(RadioButton radioButton)
        {
            radioButton.Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
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

        private void BtnPrev_Click(object sender, EventArgs e)
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

        private void BtnNext_Click(object sender, EventArgs e)
        {
            NextMonthRequested?.Invoke(this, EventArgs.Empty);

            if (currentMonth >= 12)
                return;

            currentMonth++;
            currentYear = FixedYear;
            displayedMonth = new DateTime(currentYear, currentMonth, 1);

            UpdateMonthTitle();
            BuildCalendar(currentYear, currentMonth);
            UpdateArrowState();
            BuildCategoryList();
        }

        private void UpdateMonthTitle()
        {
            currentYear = FixedYear;
            lblMonthYear.Text = $"{FixedYear}년 {currentMonth}월";
        }

        private void UpdateArrowState()
        {
            btnPrev.ForeColor = currentMonth <= 1
                ? Color.FromArgb(209, 213, 219)
                : Color.FromArgb(107, 114, 128);

            btnNext.ForeColor = currentMonth >= 12
                ? Color.FromArgb(209, 213, 219)
                : Color.FromArgb(107, 114, 128);

            btnPrev.Cursor = currentMonth <= 1 ? Cursors.Default : Cursors.Hand;
            btnNext.Cursor = currentMonth >= 12 ? Cursors.Default : Cursors.Hand;
        }

        private void BuildCalendar(int year, int month)
        {
            tlpCalendar.Controls.Clear();
            tlpCalendar.ColumnStyles.Clear();
            tlpCalendar.RowStyles.Clear();

            tlpCalendar.ColumnCount = 7;

            DateTime firstDay = new DateTime(year, month, 1);
            int startCol = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            int rowCount = (int)Math.Ceiling((startCol + daysInMonth) / 7.0);
            if (rowCount < 5)
                rowCount = 5;

            tlpCalendar.RowCount = rowCount;

            for (int i = 0; i < 7; i++)
            {
                tlpCalendar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 7F));
            }

            for (int i = 0; i < rowCount; i++)
            {
                tlpCalendar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rowCount));
            }

            int day = 1;

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    RoundedPanel cell;

                    if (row == 0 && col < startCol)
                    {
                        cell = CreateEmptyDayCell();
                    }
                    else if (day <= daysInMonth)
                    {
                        DateTime today = DateTime.Today;

                        bool isToday =
                            today.Year == FixedYear &&
                            today.Month == month &&
                            today.Day == day;

                        CalendarEventInfo eventInfo;
                        sampleEvents.TryGetValue(day, out eventInfo);

                        cell = CreateDayCell(day, eventInfo, isToday);
                        day++;
                    }
                    else
                    {
                        cell = CreateEmptyDayCell();
                    }

                    tlpCalendar.Controls.Add(cell, col, row);
                }
            }
        }

        private RoundedPanel CreateEmptyDayCell()
        {
            return new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                BorderRadius = 8,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(243, 244, 246),
                BorderSize = 1
            };
        }

        private RoundedPanel CreateDayCell(int day, CalendarEventInfo eventInfo, bool isToday)
        {
            RoundedPanel cell = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                BorderRadius = 8,
                FillColor = Color.White,
                BorderColor = Color.FromArgb(243, 244, 246),
                BorderSize = 1
            };

            if (isToday)
            {
                RoundedPanel badge = new RoundedPanel
                {
                    Size = new Size(32, 32),
                    BorderRadius = 16,
                    FillColor = Color.FromArgb(190, 24, 73),
                    BorderSize = 0
                };

                Label badgeText = new Label
                {
                    Text = day.ToString(),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("맑은 고딕", 10F, FontStyle.Bold)
                };

                badge.Controls.Add(badgeText);
                cell.Controls.Add(badge);

                cell.Resize += (s, e) =>
                {
                    badge.Left = (cell.Width - badge.Width) / 2;
                    badge.Top = 8;
                };
            }
            else
            {
                Label dayLabel = new Label
                {
                    Text = day.ToString(),
                    Dock = DockStyle.Top,
                    Height = 34,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(31, 41, 55),
                    BackColor = Color.Transparent,
                    Font = new Font("맑은 고딕", 10F, FontStyle.Bold)
                };

                cell.Controls.Add(dayLabel);
            }

            if (eventInfo != null)
            {
                CategoryInfo linkedCategory = categories.Find(c => c.Title == eventInfo.CategoryTitle);

                Color tagBackColor = linkedCategory != null
                    ? linkedCategory.EventBackColor
                    : Color.FromArgb(254, 226, 226);

                Color tagForeColor = linkedCategory != null
                    ? linkedCategory.EventForeColor
                    : Color.FromArgb(180, 83, 9);

                bool isFavoriteEvent = linkedCategory != null && linkedCategory.IsFavorite;

                RoundedPanel tag = new RoundedPanel
                {
                    Size = new Size(64, 20),
                    BorderRadius = 5,
                    FillColor = tagBackColor,
                    BorderSize = 0
                };

                Label eventLabel = new Label
                {
                    Text = isFavoriteEvent ? "★ " + eventInfo.Text : eventInfo.Text,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = tagForeColor,
                    BackColor = Color.Transparent,
                    Font = new Font("맑은 고딕", 7.5F, FontStyle.Bold)
                };

                tag.Controls.Add(eventLabel);

                tag.Cursor = Cursors.Hand;
                eventLabel.Cursor = Cursors.Hand;

                tag.Click += (s, e) => OpenEventDetail(eventInfo);
                eventLabel.Click += (s, e) => OpenEventDetail(eventInfo);

                cell.Controls.Add(tag);

                cell.Resize += (s, e) =>
                {
                    tag.Left = (cell.Width - tag.Width) / 2;
                    tag.Top = 42;
                };
            }

            return cell;
        }

        private void OpenEventDetail(CalendarEventInfo eventInfo)
        {
            EventSelected?.Invoke(this, eventInfo.EventId);
            ShowEventDetail(eventInfo);
        }

        private void BuildCategoryList()
        {
            flpCategories.Controls.Clear();

            int visibleCount = 0;

            foreach (CategoryInfo category in categories)
            {
                if (rbFav.Checked && !category.IsFavorite)
                    continue;

                AddCategoryItem(category);
                visibleCount++;
            }

            if (visibleCount == 0)
            {
                Label emptyLabel = new Label
                {
                    Text = "즐겨찾기 일정이 없습니다.",
                    Width = 198,
                    Height = 40,
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                flpCategories.Controls.Add(emptyLabel);
            }
        }

        private void AddCategoryItem(CategoryInfo category)
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
                FillColor = category.DotColor,
                BackColor = Color.Transparent
            };

            Label text = new Label
            {
                Text = category.Title,
                ForeColor = Color.FromArgb(31, 41, 55),
                BackColor = Color.Transparent,
                Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold),
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

        private Label CreateStarButton(CategoryInfo category)
        {
            bool canToggle = rbAll.Checked;

            Label starButton = new Label
            {
                Location = new Point(170, 5),
                Size = new Size(22, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Symbol", 13F, FontStyle.Regular),
                Cursor = canToggle ? Cursors.Hand : Cursors.Default
            };

            if (category.IsFavorite)
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
                starButton.Click += (s, e) => ToggleFavorite(category);
            }

            return starButton;
        }

        private void ToggleFavorite(CategoryInfo category)
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
        }

        private void ShowEventDetail(CalendarEventInfo eventInfo)
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
        }

        private void HideEventDetail()
        {
            detailPanel.Visible = false;
        }

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
        }

        private class CalendarEventInfo
        {
            public int EventId { get; private set; }
            public string Text { get; private set; }
            public string DetailTitle { get; private set; }
            public string CategoryTitle { get; private set; }
            public string TimeText { get; private set; }
            public string PlaceText { get; private set; }
            public string OriginalText { get; private set; }

            public CalendarEventInfo(
                int eventId,
                string text,
                string detailTitle,
                string categoryTitle,
                string timeText,
                string placeText,
                string originalText)
            {
                EventId = eventId;
                Text = text;
                DetailTitle = detailTitle;
                CategoryTitle = categoryTitle;
                TimeText = timeText;
                PlaceText = placeText;
                OriginalText = originalText;
            }
        }

        private class CategoryInfo
        {
            public string Title { get; private set; }
            public Color DotColor { get; private set; }
            public Color EventBackColor { get; private set; }
            public Color EventForeColor { get; private set; }
            public bool IsFavorite { get; set; }

            public CategoryInfo(
                string title,
                Color dotColor,
                Color eventBackColor,
                Color eventForeColor,
                bool isFavorite)
            {
                Title = title;
                DotColor = dotColor;
                EventBackColor = eventBackColor;
                EventForeColor = eventForeColor;
                IsFavorite = isFavorite;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void EnableFormDrag(Control control)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }
    }
}
