using System.Drawing;
using System.Windows.Forms;
using KW_Calendar.Views.Controls;

namespace KW_Calendar.Views
{
    partial class CalendarWidgetView
    {
        private System.ComponentModel.IContainer components = null;

        private Panel _titleBar;
        private Panel _dragArea;
        private Button _closeBtn;

        private CalendarGridControl _calendarGrid;

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
            _calendarGrid = new CalendarGridControl();

            SuspendLayout();

            // --- 타이틀바: 드래그 영역 + 닫기 버튼 ---
            _dragArea.Dock = DockStyle.Fill;
            _dragArea.Cursor = Cursors.SizeAll;
            _dragArea.BackColor = Color.White;
            _dragArea.MouseDown += DragArea_MouseDown;

            _closeBtn.Text = "✕";
            _closeBtn.Dock = DockStyle.Right;
            _closeBtn.Width = 28;
            _closeBtn.FlatStyle = FlatStyle.Flat;
            _closeBtn.FlatAppearance.BorderSize = 0;
            _closeBtn.BackColor = Color.White;
            _closeBtn.ForeColor = Color.FromArgb(107, 114, 128);
            // TODO: NotifyIcon 도입 후 Hide()로 변경 (별도 이슈).
            _closeBtn.Click += (_, _) => Close();

            _titleBar.Dock = DockStyle.Fill;
            _titleBar.BackColor = Color.White;
            _titleBar.Controls.Add(_dragArea);
            _titleBar.Controls.Add(_closeBtn);

            // --- 캘린더 그리드 ---
            _calendarGrid.Dock = DockStyle.Fill;
            _calendarGrid.Name = "_calendarGrid";

            // --- 루트 레이아웃 ---
            _root.Dock = DockStyle.Fill;
            _root.ColumnCount = 1;
            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _root.RowCount = 2;
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));   // 타이틀바
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 캘린더
            _root.BackColor = Color.White;
            _root.Padding = new Padding(4);

            _root.Controls.Add(_titleBar, 0, 0);
            _root.Controls.Add(_calendarGrid, 0, 1);

            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(640, 560);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "KW-Calendar Widget";

            Controls.Add(_root);

            ResumeLayout(false);
        }
    }
}
