using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KW_Calendar.Models;
using KW_Calendar.Native;

namespace KW_Calendar.Views
{
    public partial class CalendarView : Form, ICalendarView
    {
        private static readonly Font FontCategoryTitle = new("맑은 고딕", 11F, FontStyle.Bold);
        private static readonly Font FontRadio = new("맑은 고딕", 9.5F, FontStyle.Bold);
        private static readonly Font FontEmptyLabel = new("맑은 고딕", 9F, FontStyle.Bold);
        private static readonly Font FontStar = new("Segoe UI Symbol", 13F, FontStyle.Regular);
        private static readonly Font FontCategoryItem = new("맑은 고딕", 9F, FontStyle.Bold);

        private readonly ToolTip categoryTooltip = new ToolTip { ShowAlways = true };

        private IReadOnlyList<Event> events = new List<Event>();
        private IReadOnlyList<Category> modelCategories = new List<Category>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime DisplayedMonth
        {
            get => calendarGrid.DisplayedMonth;
            set => calendarGrid.DisplayedMonth = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay
        {
            get => calendarGrid.EventsByDay;
            set => calendarGrid.EventsByDay = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Event> Events
        {
            get => events;
            set => events = value ?? new List<Event>();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Category> Categories
        {
            get => modelCategories;
            set
            {
                modelCategories = value ?? new List<Category>();
                calendarGrid.Categories = modelCategories;
                if (flpCategories != null)
                {
                    BuildCategoryList();
                }
            }
        }

        public event EventHandler PreviousMonthRequested;
        public event EventHandler NextMonthRequested;
        public event EventHandler<DateOnly> AddEventRequested;
        public event EventHandler SyncRequested;
        public event EventHandler<int> EventSelected;
        public event EventHandler<int> EventFavoriteToggleRequested;
        public event EventHandler<int> CategoryFavoriteToggleRequested;

        public CalendarView()
        {
            InitializeComponent();

            ApplySideDesign();
            InitializeTitleBar();

            rbAll.CheckedChanged += FilterRadio_CheckedChanged;
            rbFav.CheckedChanged += FilterRadio_CheckedChanged;

            // 그리드 이벤트를 View 이벤트로 패스스루.
            calendarGrid.PreviousMonthRequested += (s, e) => PreviousMonthRequested?.Invoke(this, e);
            calendarGrid.NextMonthRequested += (s, e) => NextMonthRequested?.Invoke(this, e);
            calendarGrid.EventSelected += (s, id) => EventSelected?.Invoke(this, id);
            calendarGrid.DaySelected += (s, date) => AddEventRequested?.Invoke(this, date);

            lblDetailClose.MouseEnter += (s, e) =>
            {
                lblDetailClose.ForeColor = Color.FromArgb(190, 24, 73);
            };

            lblDetailClose.MouseLeave += (s, e) =>
            {
                lblDetailClose.ForeColor = Color.FromArgb(107, 114, 128);
            };
        }

        private void ApplySideDesign()
        {
            BackColor = Color.FromArgb(238, 239, 241);

            sideArea.BackColor = Color.FromArgb(249, 250, 251);

            StyleRadioButton(rbFav);
            StyleRadioButton(rbAll);

            lblCategoryTitle.Font = FontCategoryTitle;
            lblCategoryTitle.ForeColor = Color.FromArgb(31, 41, 55);
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

        private const int ResizeBorder = 6;

        // borderless(FormBorderStyle.None) Form은 WinForms가 WS_MINIMIZEBOX 비트를
        // 윈도우 스타일에 안 넣음. 그 결과 Win+D("바탕화면 보기")가 이 창을 건너뜀.
        // 메인 창은 Win+D에 정상 반응해야 하므로 비트를 직접 켠다.
        // 참고: https://devblogs.microsoft.com/oldnewthing/20241021-00/?p=110393
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                var cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowHelpers.ApplyRoundedCorners(Handle);
        }

        // X 버튼 / Alt+F4는 종료가 아닌 트레이로 숨김. 실제 종료는 트레이 메뉴
        // "종료"가 부르는 Application.Exit() → CloseReason.ApplicationExitCall.
        // OS 종료/세션 종료는 막지 않는다.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            base.OnFormClosing(e);
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

        private const int WM_SETREDRAW = 0x000B;

        private void BuildCategoryList()
        {
            SendMessage(flpCategories.Handle, WM_SETREDRAW, 0, 0);
            flpCategories.SuspendLayout();
            try
            {
                flpCategories.Controls.Clear();

                int visibleCount = 0;

                foreach (Category category in modelCategories)
                {
                    if (rbFav.Checked && !category.IsFavorited)
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

        private void AddCategoryItem(Category category)
        {
            RoundedPanel item = new RoundedPanel
            {
                Width = 224,
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
                Font = FontCategoryItem,
                Location = new Point(42, 9),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            categoryTooltip.SetToolTip(text, category.Name);

            Label starButton = CreateStarButton(category);

            item.Controls.Add(dot);
            item.Controls.Add(text);
            item.Controls.Add(starButton);

            flpCategories.Controls.Add(item);
        }

        private Label CreateStarButton(Category category)
        {
            bool canToggle = rbAll.Checked;

            Label starButton = new Label
            {
                Location = new Point(196, 5),
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

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
