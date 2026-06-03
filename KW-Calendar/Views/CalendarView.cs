using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KW_Calendar.Models;
using System.ComponentModel;//추가

namespace KW_Calendar.Views
{
    public partial class CalendarView : Form, ICalendarView
    {
        private const int FixedYear = 2026;

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

                        /*현 코드 내에 sample데이터만 사용하게 됨 -> Present에서 주는 데이터 사용하게*/
                        DateOnly date = new DateOnly(year, month, day);

                        if (!eventsByDay.TryGetValue(date, out var dayEvents))
                        {
                            dayEvents = new List<Event>();
                        }

                        cell = CreateDayCell(day, dayEvents, isToday);
                        day++;
                        //
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

        private RoundedPanel CreateDayCell(int day, IReadOnlyList<Event> dayEvents, bool isToday)
        {//IReadOnlyList<Event> dayEvents 로 수정
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

            //이벤트 if문 수정
            if (dayEvents.Count > 0)
            {
                Event firstEvent = dayEvents[0];

                Category? linkedCategory = modelCategories
                    .FirstOrDefault(c => c.Id == firstEvent.CategoryId);

                bool isFavoriteEvent =
                    firstEvent.IsFavorited ||
                    (linkedCategory != null && linkedCategory.IsFavorited);

                RoundedPanel tag = new RoundedPanel
                {
                    Size = new Size(64, 20),
                    BorderRadius = 5,
                    FillColor = GetCategoryBackColor(linkedCategory),
                    BorderSize = 0
                };

                Label eventLabel = new Label
                {
                    Text = isFavoriteEvent ? "★ " + ShortenTitle(firstEvent.Title) : ShortenTitle(firstEvent.Title),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = GetCategoryForeColor(linkedCategory),
                    BackColor = Color.Transparent,
                    Font = new Font("맑은 고딕", 7.5F, FontStyle.Bold)
                };

                tag.Controls.Add(eventLabel);

                tag.Cursor = Cursors.Hand;
                eventLabel.Cursor = Cursors.Hand;

                tag.Click += (s, e) => EventSelected?.Invoke(this, firstEvent.Id);
                eventLabel.Click += (s, e) => EventSelected?.Invoke(this, firstEvent.Id);

                cell.Controls.Add(tag);

                cell.Resize += (s, e) =>
                {
                    tag.Left = (cell.Width - tag.Width) / 2;
                    tag.Top = 42;
                };
            }
            return cell;


        }
        //보조 메서드 추가
        private string ShortenTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "";

            return title.Length > 3 ? title.Substring(0, 3) + "..." : title;
        }

        //삭제
       /* private void OpenEventDetail(CalendarEventInfo eventInfo)
        {
            EventSelected?.Invoke(this, eventInfo.EventId);
            ShowEventDetail(eventInfo);
        }*/

        private void BuildCategoryList()
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
                    Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                flpCategories.Controls.Add(emptyLabel);
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
                Font = new Font("Segoe UI Symbol", 13F, FontStyle.Regular),
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
