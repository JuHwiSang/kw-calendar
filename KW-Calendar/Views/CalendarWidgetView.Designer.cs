using System.Drawing;
using System.Windows.Forms;
using KW_Calendar.Views.Controls;

namespace KW_Calendar.Views
{
    partial class CalendarWidgetView
    {
        private System.ComponentModel.IContainer components = null;

        private CustomTitleBar _titleBar;
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
            _titleBar = new CustomTitleBar
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                TitleText = "KW Calendar",
                ShowMinimize = false,
                ShowMaximize = false,
                // Control 기본 Margin이 (3,3,3,3)이라 TableLayoutPanel 셀 안에서
                // 좌우상하 3px씩 들어간다. 위젯에선 가장자리에 꽉 붙어야 하므로 0.
                Margin = Padding.Empty,
            };
            _calendarGrid = new CalendarGridControl
            {
                Margin = Padding.Empty,
            };

            SuspendLayout();

            // --- 캘린더 그리드 ---
            _calendarGrid.Dock = DockStyle.Fill;
            _calendarGrid.Name = "_calendarGrid";

            // --- 루트 레이아웃 ---
            _root.Dock = DockStyle.Fill;
            _root.ColumnCount = 1;
            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _root.RowCount = 2;
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // 타이틀바
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 캘린더
            _root.BackColor = Color.White;

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
