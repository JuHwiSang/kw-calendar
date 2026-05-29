using CalendarApp;
using System.Drawing;
using System.Windows.Forms;



namespace CalendarApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private RoundedPanel mainCard;
        private Panel leftArea;
        private Panel sideArea;
        private Panel divider;

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

        private RadioButton rbFav;
        private RadioButton rbAll;
        private Label lblCategoryTitle;
        private FlowLayoutPanel flpCategories;

        //일정 상세 페이지
        private RoundedPanel detailPanel;
        private Panel detailHeader;
        private Label lblDetailStar;
        private Label lblDetailTitle;
        private Label lblDetailClose;
        private Panel detailBody;

        //상세페이지 내부(detailBody)
        private Panel detailContent;
        private RoundedPanel iconTime;
        private RoundedPanel iconPlace;
        private RoundedPanel iconOriginal;

        private Label lblTimeIcon;
        private Label lblPlaceIcon;
        private Label lblOriginalIcon;

        private Label lblDetailTimeTitle;
        private Label lblDetailTimeValue;

        private Label lblDetailPlaceTitle;
        private Label lblDetailPlaceValue;

        private Label lblDetailOriginalTitle;
        private RoundedPanel originalTextBox;
        private Label lblDetailOriginalValue;

        private RoundedPanel btnOriginalPage;
        private Label lblOriginalPageText;
        //
        private Panel detailScrollTrack;
        private RoundedPanel detailScrollThumb;
        private Label lblScrollUp;
        private Label lblScrollDown;
        ////

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
            mainCard = new RoundedPanel();
            leftArea = new Panel();
            sideArea = new Panel();
            divider = new Panel();

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

            rbFav = new RadioButton();
            rbAll = new RadioButton();
            lblCategoryTitle = new Label();
            flpCategories = new FlowLayoutPanel();

            //상세 일정 페이지
            detailPanel = new RoundedPanel();
            detailHeader = new Panel();
            lblDetailStar = new Label();
            lblDetailTitle = new Label();
            lblDetailClose = new Label();
            detailBody = new Panel();
            //상세 페이지 내부(detailBody)
            detailContent = new Panel();
            iconTime = new RoundedPanel();
            iconPlace = new RoundedPanel();
            iconOriginal = new RoundedPanel();

            lblTimeIcon = new Label();
            lblPlaceIcon = new Label();
            lblOriginalIcon = new Label();

            lblDetailTimeTitle = new Label();
            lblDetailTimeValue = new Label();

            lblDetailPlaceTitle = new Label();
            lblDetailPlaceValue = new Label();

            lblDetailOriginalTitle = new Label();
            originalTextBox = new RoundedPanel();
            lblDetailOriginalValue = new Label();

            btnOriginalPage = new RoundedPanel();
            lblOriginalPageText = new Label();
            //
            detailScrollTrack = new Panel();
            detailScrollThumb = new RoundedPanel();
            lblScrollUp = new Label();
            lblScrollDown = new Label();
            ////////

            SuspendLayout();
            mainCard.SuspendLayout();
            leftArea.SuspendLayout();
            sideArea.SuspendLayout();
            panelHeader.SuspendLayout();
            tlpDaysOfWeek.SuspendLayout();

            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(238, 239, 241);
            ClientSize = new Size(884, 692);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Name = "MainForm";
            Text = "KW-calendar";

            // 
            // mainCard
            // 
            mainCard.Location = new Point(26, 36);
            mainCard.Size = new Size(832, 624);
            mainCard.BorderRadius = 28;
            mainCard.BorderSize = 0;
            mainCard.FillColor = Color.White;
            mainCard.BackColor = Color.Transparent;
            mainCard.Name = "mainCard";

            // 
            // leftArea
            // 
            leftArea.BackColor = Color.White;
            leftArea.Location = new Point(0, 0);
            leftArea.Size = new Size(588, 624);
            leftArea.Name = "leftArea";

            // 
            // sideArea
            // 
            sideArea.BackColor = Color.FromArgb(249, 250, 251);
            sideArea.Location = new Point(588, 0);
            sideArea.Size = new Size(244, 624);
            sideArea.Name = "sideArea";

            // 
            // divider
            // 
            divider.BackColor = Color.FromArgb(229, 231, 235);
            divider.Location = new Point(587, 0);
            divider.Size = new Size(1, 624);
            divider.Name = "divider";

            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.White;
            panelHeader.Location = new Point(26, 28);
            panelHeader.Size = new Size(535, 58);
            panelHeader.Name = "panelHeader";
            // 
            // btnPrev
            // 
            btnPrev.Location = new Point(14, 7);
            btnPrev.Size = new Size(42, 42);
            btnPrev.Name = "btnPrev";
            btnPrev.Text = "‹";
            btnPrev.TextAlign = ContentAlignment.MiddleCenter;
            btnPrev.BackColor = Color.White;
            btnPrev.Cursor = Cursors.Hand;

            // 
            // lblMonthYear
            // 
            lblMonthYear.Location = new Point(143, 7);
            lblMonthYear.Size = new Size(250, 44);
            lblMonthYear.Name = "lblMonthYear";
            lblMonthYear.Text = "2024년 5월";
            lblMonthYear.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(479, 7);
            btnNext.Size = new Size(42, 42);
            btnNext.Name = "btnNext";
            btnNext.Text = "›";
            btnNext.TextAlign = ContentAlignment.MiddleCenter;
            btnNext.BackColor = Color.White;
            btnNext.Cursor = Cursors.Hand;

            panelHeader.Controls.Add(btnPrev);
            panelHeader.Controls.Add(lblMonthYear);
            panelHeader.Controls.Add(btnNext);

            // 
            // tlpDaysOfWeek
            // 
            tlpDaysOfWeek.BackColor = Color.White;
            tlpDaysOfWeek.ColumnCount = 7;
            tlpDaysOfWeek.RowCount = 1;
            tlpDaysOfWeek.Location = new Point(26, 96);
            tlpDaysOfWeek.Size = new Size(535, 30);
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
            tlpCalendar.Location = new Point(26, 136);
            tlpCalendar.Size = new Size(535, 482);
            tlpCalendar.Name = "tlpCalendar";

            // 
            // rbFav
            // 
            rbFav.AutoSize = true;
            rbFav.Location = new Point(20, 34);
            rbFav.Name = "rbFav";
            rbFav.Size = new Size(130, 24);
            rbFav.TabIndex = 0;
            rbFav.TabStop = true;
            rbFav.Text = "즐겨찾기 일정";
            rbFav.UseVisualStyleBackColor = false;

            // 
            // rbAll
            // 
            rbAll.AutoSize = true;
            rbAll.Checked = true;
            rbAll.Location = new Point(20, 56);
            rbAll.Name = "rbAll";
            rbAll.Size = new Size(100, 24);
            rbAll.TabIndex = 1;
            rbAll.TabStop = true;
            rbAll.Text = "전체 일정";
            rbAll.UseVisualStyleBackColor = false;

            // 
            // lblCategoryTitle
            // 
            lblCategoryTitle.Location = new Point(22, 100);
            lblCategoryTitle.Size = new Size(190, 26);
            lblCategoryTitle.Name = "lblCategoryTitle";
            lblCategoryTitle.Text = "일정 카테고리";
            lblCategoryTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblCategoryTitle.BackColor = Color.Transparent;

            // 
            // flpCategories
            // 
            flpCategories.Location = new Point(20, 138);
            flpCategories.Size = new Size(204, 430);
            flpCategories.Name = "flpCategories";
            flpCategories.BackColor = Color.FromArgb(249, 250, 251);
            flpCategories.FlowDirection = FlowDirection.TopDown;
            flpCategories.WrapContents = false;
            flpCategories.AutoScroll = false;

            sideArea.Controls.Add(rbFav);
            sideArea.Controls.Add(rbAll);
            sideArea.Controls.Add(lblCategoryTitle);
            sideArea.Controls.Add(flpCategories);

            //일정 상세 페이지
            // 
            // detailPanel
            // 
            detailPanel.Location = new Point(76, 36);
            detailPanel.Size = new Size(475, 548);
            detailPanel.BorderRadius = 16;
            detailPanel.BorderSize = 1;
            detailPanel.BorderColor = Color.FromArgb(229, 231, 235);
            detailPanel.FillColor = Color.White;
            detailPanel.BackColor = Color.Transparent;
            detailPanel.Visible = false;
            detailPanel.Name = "detailPanel";

            // 
            // detailHeader
            // 
            detailHeader.Location = new Point(0, 0);
            detailHeader.Size = new Size(475, 74);
            detailHeader.BackColor = Color.FromArgb(254, 226, 226);
            detailHeader.Name = "detailHeader";

            // 
            // lblDetailStar
            // 
            lblDetailStar.Location = new Point(20, 19);
            lblDetailStar.Size = new Size(34, 34);
            lblDetailStar.Text = "★";
            lblDetailStar.TextAlign = ContentAlignment.MiddleCenter;
            lblDetailStar.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular);
            lblDetailStar.ForeColor = Color.FromArgb(250, 204, 21);
            lblDetailStar.BackColor = Color.Transparent;
            lblDetailStar.Name = "lblDetailStar";

            // 
            // lblDetailTitle
            // 
            lblDetailTitle.Location = new Point(62, 16);
            lblDetailTitle.Size = new Size(330, 42);
            lblDetailTitle.Text = "일정 이름";
            lblDetailTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblDetailTitle.Font = new Font("맑은 고딕", 15F, FontStyle.Bold);
            lblDetailTitle.ForeColor = Color.FromArgb(31, 41, 55);
            lblDetailTitle.BackColor = Color.Transparent;
            lblDetailTitle.Name = "lblDetailTitle";

            // 
            // lblDetailClose
            // 
            lblDetailClose.Location = new Point(428, 18);
            lblDetailClose.Size = new Size(34, 34);
            lblDetailClose.Text = "×";
            lblDetailClose.TextAlign = ContentAlignment.MiddleCenter;
            lblDetailClose.Font = new Font("Segoe UI", 18F, FontStyle.Regular);
            lblDetailClose.ForeColor = Color.FromArgb(107, 114, 128);
            lblDetailClose.BackColor = Color.Transparent;
            lblDetailClose.Cursor = Cursors.Hand;
            lblDetailClose.Name = "lblDetailClose";

            // 
            // detailBody
            // 
            detailBody.Location = new Point(0, 74);
            detailBody.Size = new Size(453, 474);
            detailBody.BackColor = Color.White;
            detailBody.AutoScroll = false;
            detailBody.Name = "detailBody";

            detailContent.Location = new Point(0, 0);
            detailContent.Size = new Size(453, 620);
            detailContent.BackColor = Color.White;
            detailContent.Name = "detailContent";

            //상세 페이지 내부(detailbody)
            // 
            // iconTime
            // 
            iconTime.Location = new Point(20, 24);
            iconTime.Size = new Size(42, 42);
            iconTime.BorderRadius = 10;
            iconTime.BorderSize = 0;
            iconTime.FillColor = Color.FromArgb(255, 228, 230);
            iconTime.BackColor = Color.Transparent;

            lblTimeIcon.Dock = DockStyle.Fill;
            lblTimeIcon.Text = "◷";
            lblTimeIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblTimeIcon.Font = new Font("Segoe UI Symbol", 17F, FontStyle.Regular);
            lblTimeIcon.ForeColor = Color.FromArgb(244, 63, 94);
            lblTimeIcon.BackColor = Color.Transparent;

            iconTime.Controls.Add(lblTimeIcon);

            // 
            // lblDetailTimeTitle
            // 
            lblDetailTimeTitle.Location = new Point(78, 24);
            lblDetailTimeTitle.Size = new Size(300, 22);
            lblDetailTimeTitle.Text = "일시";
            lblDetailTimeTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
            lblDetailTimeTitle.ForeColor = Color.FromArgb(107, 114, 128);
            lblDetailTimeTitle.BackColor = Color.Transparent;

            // 
            // lblDetailTimeValue
            // 
            lblDetailTimeValue.Location = new Point(78, 47);
            lblDetailTimeValue.Size = new Size(320, 26);
            lblDetailTimeValue.Text = "14:00 - 16:00";
            lblDetailTimeValue.Font = new Font("맑은 고딕", 10.5F, FontStyle.Bold);
            lblDetailTimeValue.ForeColor = Color.FromArgb(17, 24, 39);
            lblDetailTimeValue.BackColor = Color.Transparent;

            // 
            // iconPlace
            // 
            iconPlace.Location = new Point(20, 96);
            iconPlace.Size = new Size(42, 42);
            iconPlace.BorderRadius = 10;
            iconPlace.BorderSize = 0;
            iconPlace.FillColor = Color.FromArgb(243, 244, 246);
            iconPlace.BackColor = Color.Transparent;

            lblPlaceIcon.Dock = DockStyle.Fill;
            lblPlaceIcon.Text = "⌖";
            lblPlaceIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblPlaceIcon.Font = new Font("Segoe UI Symbol", 16F, FontStyle.Regular);
            lblPlaceIcon.ForeColor = Color.FromArgb(107, 114, 128);
            lblPlaceIcon.BackColor = Color.Transparent;

            iconPlace.Controls.Add(lblPlaceIcon);

            // 
            // lblDetailPlaceTitle
            // 
            lblDetailPlaceTitle.Location = new Point(78, 96);
            lblDetailPlaceTitle.Size = new Size(300, 22);
            lblDetailPlaceTitle.Text = "장소";
            lblDetailPlaceTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
            lblDetailPlaceTitle.ForeColor = Color.FromArgb(107, 114, 128);
            lblDetailPlaceTitle.BackColor = Color.Transparent;

            // 
            // lblDetailPlaceValue
            // 
            lblDetailPlaceValue.Location = new Point(78, 119);
            lblDetailPlaceValue.Size = new Size(320, 26);
            lblDetailPlaceValue.Text = "소프트웨어관 301호";
            lblDetailPlaceValue.Font = new Font("맑은 고딕", 10.5F, FontStyle.Regular);
            lblDetailPlaceValue.ForeColor = Color.FromArgb(17, 24, 39);
            lblDetailPlaceValue.BackColor = Color.Transparent;

            // 
            // iconOriginal
            // 
            iconOriginal.Location = new Point(20, 174);
            iconOriginal.Size = new Size(42, 42);
            iconOriginal.BorderRadius = 10;
            iconOriginal.BorderSize = 0;
            iconOriginal.FillColor = Color.FromArgb(243, 244, 246);
            iconOriginal.BackColor = Color.Transparent;

            lblOriginalIcon.Dock = DockStyle.Fill;
            lblOriginalIcon.Text = "≡";
            lblOriginalIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblOriginalIcon.Font = new Font("Segoe UI", 17F, FontStyle.Regular);
            lblOriginalIcon.ForeColor = Color.FromArgb(107, 114, 128);
            lblOriginalIcon.BackColor = Color.Transparent;

            iconOriginal.Controls.Add(lblOriginalIcon);

            // 
            // lblDetailOriginalTitle
            // 
            lblDetailOriginalTitle.Location = new Point(78, 174);
            lblDetailOriginalTitle.Size = new Size(300, 24);
            lblDetailOriginalTitle.Text = "상세 내용 (원문)";
            lblDetailOriginalTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
            lblDetailOriginalTitle.ForeColor = Color.FromArgb(107, 114, 128);
            lblDetailOriginalTitle.BackColor = Color.Transparent;

            // 
            // originalTextBox
            // 
            originalTextBox.Location = new Point(78, 204);
            originalTextBox.Size = new Size(345, 225);
            originalTextBox.BorderRadius = 14;
            originalTextBox.BorderSize = 1;
            originalTextBox.BorderColor = Color.FromArgb(229, 231, 235);
            originalTextBox.FillColor = Color.FromArgb(249, 250, 251);
            originalTextBox.BackColor = Color.Transparent;

            // 
            // lblDetailOriginalValue
            // 
            lblDetailOriginalValue.Location = new Point(16, 14);
            lblDetailOriginalValue.Size = new Size(313, 195);
            lblDetailOriginalValue.Text = "";
            lblDetailOriginalValue.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
            lblDetailOriginalValue.ForeColor = Color.FromArgb(31, 41, 55);
            lblDetailOriginalValue.BackColor = Color.Transparent;
            lblDetailOriginalValue.TextAlign = ContentAlignment.TopLeft;

            originalTextBox.Controls.Add(lblDetailOriginalValue);

            // 
            // btnOriginalPage
            // 
            btnOriginalPage.Location = new Point(20, 460);
            btnOriginalPage.Size = new Size(403, 48);
            btnOriginalPage.BorderRadius = 12;
            btnOriginalPage.BorderSize = 0;
            btnOriginalPage.FillColor = Color.FromArgb(31, 41, 55);
            btnOriginalPage.BackColor = Color.Transparent;
            btnOriginalPage.Cursor = Cursors.Hand;

            // 
            // lblOriginalPageText
            // 
            lblOriginalPageText.Dock = DockStyle.Fill;
            lblOriginalPageText.Text = "↗ 원문 페이지로 이동";
            lblOriginalPageText.TextAlign = ContentAlignment.MiddleCenter;
            lblOriginalPageText.Font = new Font("맑은 고딕", 10.5F, FontStyle.Bold);
            lblOriginalPageText.ForeColor = Color.White;
            lblOriginalPageText.BackColor = Color.Transparent;
            lblOriginalPageText.Cursor = Cursors.Hand;

            btnOriginalPage.Controls.Add(lblOriginalPageText);

            detailContent.Controls.Add(iconTime);
            detailContent.Controls.Add(lblDetailTimeTitle);
            detailContent.Controls.Add(lblDetailTimeValue);

            detailContent.Controls.Add(iconPlace);
            detailContent.Controls.Add(lblDetailPlaceTitle);
            detailContent.Controls.Add(lblDetailPlaceValue);

            detailContent.Controls.Add(iconOriginal);
            detailContent.Controls.Add(lblDetailOriginalTitle);
            detailContent.Controls.Add(originalTextBox);
            detailContent.Controls.Add(btnOriginalPage);

            detailBody.Controls.Add(detailContent);
            //

            // 
            // detailScrollTrack
            // 
            detailScrollTrack.Location = new Point(453, 74);
            detailScrollTrack.Size = new Size(22, 474);
            detailScrollTrack.BackColor = Color.White;
            detailScrollTrack.Name = "detailScrollTrack";

            // 
            // lblScrollUp
            // 
            lblScrollUp.Location = new Point(0, 0);
            lblScrollUp.Size = new Size(22, 22);
            lblScrollUp.Text = "▲";
            lblScrollUp.TextAlign = ContentAlignment.MiddleCenter;
            lblScrollUp.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblScrollUp.ForeColor = Color.FromArgb(156, 163, 175);
            lblScrollUp.BackColor = Color.White;
            lblScrollUp.Name = "lblScrollUp";

            // 
            // detailScrollThumb
            // 
            detailScrollThumb.Location = new Point(8, 34);
            detailScrollThumb.Size = new Size(6, 340);
            detailScrollThumb.BorderRadius = 3;
            detailScrollThumb.BorderSize = 0;
            detailScrollThumb.FillColor = Color.FromArgb(150, 150, 150);
            detailScrollThumb.BackColor = Color.Transparent;
            detailScrollThumb.Name = "detailScrollThumb";

            // 
            // lblScrollDown
            // 
            lblScrollDown.Location = new Point(0, 452);
            lblScrollDown.Size = new Size(22, 22);
            lblScrollDown.Text = "▼";
            lblScrollDown.TextAlign = ContentAlignment.MiddleCenter;
            lblScrollDown.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblScrollDown.ForeColor = Color.FromArgb(156, 163, 175);
            lblScrollDown.BackColor = Color.White;
            lblScrollDown.Name = "lblScrollDown";

            detailHeader.Controls.Add(lblDetailStar);
            detailHeader.Controls.Add(lblDetailTitle);
            detailHeader.Controls.Add(lblDetailClose);

            detailScrollTrack.Controls.Add(lblScrollUp);
            detailScrollTrack.Controls.Add(detailScrollThumb);
            detailScrollTrack.Controls.Add(lblScrollDown);

            detailPanel.Controls.Add(detailBody);
            detailPanel.Controls.Add(detailScrollTrack);
            detailPanel.Controls.Add(detailHeader);
            //

            leftArea.Controls.Add(panelHeader);
            leftArea.Controls.Add(tlpDaysOfWeek);
            leftArea.Controls.Add(tlpCalendar);
            //일정 상세 페이지
            leftArea.Controls.Add(detailPanel); 
            //
            mainCard.Controls.Add(leftArea);
            mainCard.Controls.Add(divider);
            mainCard.Controls.Add(sideArea);

            Controls.Add(mainCard);

            tlpDaysOfWeek.ResumeLayout(false);
            panelHeader.ResumeLayout(false);
            sideArea.ResumeLayout(false);
            leftArea.ResumeLayout(false);
            mainCard.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}