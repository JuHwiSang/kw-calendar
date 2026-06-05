using System.Drawing;
using System.Windows.Forms;

namespace KW_Calendar.Views.Controls
{
    partial class CalendarGridControl
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelHeader;
        private Label btnPrev;
        private Label btnNext;
        private Label lblMonthYear;

        private TableLayoutPanel tlpDaysOfWeek;
        private Label lblSun;
        private Label lblMon;
        private Label lblTue;
        private Label lblWed;
        private Label lblThu;
        private Label lblFri;
        private Label lblSat;

        private TableLayoutPanel tlpCalendar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            panelHeader = new Panel();
            btnPrev = new Label();
            btnNext = new Label();
            lblMonthYear = new Label();

            tlpDaysOfWeek = new TableLayoutPanel();
            lblSun = new Label();
            lblMon = new Label();
            lblTue = new Label();
            lblWed = new Label();
            lblThu = new Label();
            lblFri = new Label();
            lblSat = new Label();

            tlpCalendar = new TableLayoutPanel();

            SuspendLayout();
            panelHeader.SuspendLayout();
            tlpDaysOfWeek.SuspendLayout();

            //
            // panelHeader
            //
            panelHeader.BackColor = Color.White;
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 58;
            panelHeader.Name = "panelHeader";

            //
            // btnPrev
            //
            btnPrev.Dock = DockStyle.Left;
            btnPrev.Width = 56;
            btnPrev.Name = "btnPrev";
            btnPrev.Text = "‹";
            btnPrev.TextAlign = ContentAlignment.MiddleCenter;
            btnPrev.BackColor = Color.White;
            btnPrev.Cursor = Cursors.Hand;

            //
            // lblMonthYear
            //
            lblMonthYear.Dock = DockStyle.Fill;
            lblMonthYear.Name = "lblMonthYear";
            lblMonthYear.Text = "----";
            lblMonthYear.TextAlign = ContentAlignment.MiddleCenter;

            //
            // btnNext
            //
            btnNext.Dock = DockStyle.Right;
            btnNext.Width = 56;
            btnNext.Name = "btnNext";
            btnNext.Text = "›";
            btnNext.TextAlign = ContentAlignment.MiddleCenter;
            btnNext.BackColor = Color.White;
            btnNext.Cursor = Cursors.Hand;

            panelHeader.Controls.Add(lblMonthYear);
            panelHeader.Controls.Add(btnPrev);
            panelHeader.Controls.Add(btnNext);

            //
            // tlpDaysOfWeek
            //
            tlpDaysOfWeek.BackColor = Color.White;
            tlpDaysOfWeek.ColumnCount = 7;
            tlpDaysOfWeek.RowCount = 1;
            tlpDaysOfWeek.Dock = DockStyle.Top;
            tlpDaysOfWeek.Height = 30;
            tlpDaysOfWeek.Name = "tlpDaysOfWeek";

            for (int i = 0; i < 7; i++)
            {
                tlpDaysOfWeek.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 7F));
            }
            tlpDaysOfWeek.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            lblSun.Text = "일";
            lblMon.Text = "월";
            lblTue.Text = "화";
            lblWed.Text = "수";
            lblThu.Text = "목";
            lblFri.Text = "금";
            lblSat.Text = "토";

            Label[] dayLabels = { lblSun, lblMon, lblTue, lblWed, lblThu, lblFri, lblSat };
            for (int i = 0; i < dayLabels.Length; i++)
            {
                dayLabels[i].Dock = DockStyle.Fill;
                dayLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                dayLabels[i].BackColor = Color.White;
                tlpDaysOfWeek.Controls.Add(dayLabels[i], i, 0);
            }

            //
            // tlpCalendar
            //
            tlpCalendar.BackColor = Color.White;
            tlpCalendar.ColumnCount = 7;
            tlpCalendar.RowCount = 5;
            tlpCalendar.Dock = DockStyle.Fill;
            tlpCalendar.Name = "tlpCalendar";

            //
            // CalendarGridControl
            //
            BackColor = Color.White;
            Name = "CalendarGridControl";

            // Dock 처리는 add 역순. Fill인 tlpCalendar를 먼저 add → 마지막에 처리되며 남은 공간 차지.
            Controls.Add(tlpCalendar);
            Controls.Add(tlpDaysOfWeek);
            Controls.Add(panelHeader);

            tlpDaysOfWeek.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
