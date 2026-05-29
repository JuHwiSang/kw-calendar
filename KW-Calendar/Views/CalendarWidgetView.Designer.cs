namespace KW_Calendar.Views
{
    partial class CalendarWidgetView
    {
        private System.ComponentModel.IContainer components = null;

        private Panel _titleBar;
        private Panel _dragArea;
        private Button _closeBtn;

        private Panel _headerPanel;
        private Button _prevBtn;
        private Label _monthLabel;
        private Button _nextBtn;

        private Label _eventsSectionLabel;
        private ListBox _eventsList;

        private Label _favEventsSectionLabel;
        private ListBox _favEventsList;

        private Label _favCategoriesSectionLabel;
        private ListBox _favCategoriesList;

        private TableLayoutPanel _root;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _root = new TableLayoutPanel();
            _titleBar = new Panel();
            _dragArea = new Panel();
            _closeBtn = new Button();
            _headerPanel = new Panel();
            _prevBtn = new Button();
            _monthLabel = new Label();
            _nextBtn = new Button();
            _eventsSectionLabel = new Label();
            _eventsList = new ListBox();
            _favEventsSectionLabel = new Label();
            _favEventsList = new ListBox();
            _favCategoriesSectionLabel = new Label();
            _favCategoriesList = new ListBox();

            SuspendLayout();

            // --- 타이틀바: 드래그 영역 + 닫기 버튼 ---
            _dragArea.Dock = DockStyle.Fill;
            _dragArea.Cursor = Cursors.SizeAll;
            _dragArea.MouseDown += DragArea_MouseDown;

            _closeBtn.Text = "✕";
            _closeBtn.Dock = DockStyle.Right;
            _closeBtn.Width = 28;
            _closeBtn.FlatStyle = FlatStyle.Flat;
            // TODO: NotifyIcon 도입 후 Hide()로 변경 (별도 이슈).
            _closeBtn.Click += (_, _) => Close();

            _titleBar.Height = 22;
            _titleBar.Dock = DockStyle.Fill;
            _titleBar.Controls.Add(_dragArea);
            _titleBar.Controls.Add(_closeBtn);

            // --- 헤더: < / 월 / > ---
            _prevBtn.Text = "<";
            _prevBtn.Dock = DockStyle.Left;
            _prevBtn.Width = 32;
            _prevBtn.Click += (_, _) => PreviousMonthRequested?.Invoke(this, EventArgs.Empty);

            _nextBtn.Text = ">";
            _nextBtn.Dock = DockStyle.Right;
            _nextBtn.Width = 32;
            _nextBtn.Click += (_, _) => NextMonthRequested?.Invoke(this, EventArgs.Empty);

            _monthLabel.Dock = DockStyle.Fill;
            _monthLabel.TextAlign = ContentAlignment.MiddleCenter;
            _monthLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _monthLabel.Text = "----";

            _headerPanel.Height = 30;
            _headerPanel.Dock = DockStyle.Fill;
            _headerPanel.Controls.Add(_monthLabel);
            _headerPanel.Controls.Add(_prevBtn);
            _headerPanel.Controls.Add(_nextBtn);

            // --- 섹션 라벨 + 리스트 3쌍 ---
            ConfigureSectionLabel(_eventsSectionLabel, "이번 달 이벤트");
            ConfigureList(_eventsList);
            _eventsList.SelectedIndexChanged += EventsList_SelectedIndexChanged;

            ConfigureSectionLabel(_favEventsSectionLabel, "즐겨찾기 이벤트");
            ConfigureList(_favEventsList);
            _favEventsList.SelectedIndexChanged += FavEventsList_SelectedIndexChanged;

            ConfigureSectionLabel(_favCategoriesSectionLabel, "즐겨찾기 카테고리");
            ConfigureList(_favCategoriesList);
            _favCategoriesList.SelectedIndexChanged += FavCategoriesList_SelectedIndexChanged;

            // --- 루트 레이아웃 ---
            _root.Dock = DockStyle.Fill;
            _root.ColumnCount = 1;
            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _root.RowCount = 8;
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // 타이틀바
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 헤더
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // 섹션 라벨
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // 이번 달 이벤트 (큰 영역)
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // 섹션 라벨
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));  // 즐겨찾기 이벤트
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // 섹션 라벨
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));  // 즐겨찾기 카테고리
            _root.Padding = new Padding(6);

            _root.Controls.Add(_titleBar, 0, 0);
            _root.Controls.Add(_headerPanel, 0, 1);
            _root.Controls.Add(_eventsSectionLabel, 0, 2);
            _root.Controls.Add(_eventsList, 0, 3);
            _root.Controls.Add(_favEventsSectionLabel, 0, 4);
            _root.Controls.Add(_favEventsList, 0, 5);
            _root.Controls.Add(_favCategoriesSectionLabel, 0, 6);
            _root.Controls.Add(_favCategoriesList, 0, 7);

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(320, 480);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "KW-Calendar Widget";

            Controls.Add(_root);

            ResumeLayout(false);
        }

        private static void ConfigureSectionLabel(Label label, string text)
        {
            label.Text = text;
            label.Dock = DockStyle.Fill;
            label.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            label.ForeColor = Color.Gray;
            label.TextAlign = ContentAlignment.BottomLeft;
        }

        private static void ConfigureList(ListBox list)
        {
            list.Dock = DockStyle.Fill;
            list.BorderStyle = BorderStyle.None;
            list.IntegralHeight = false;
        }
    }
}
