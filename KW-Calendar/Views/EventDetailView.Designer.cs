namespace KW_Calendar.Views;

partial class EventDetailView
{
    private System.ComponentModel.IContainer components = null;

    private Panel scrollHost;
    private FlowLayoutPanel sectionStack;

    private Panel timeSection;
    private RoundedPanel timeIconBox;
    private Label lblTimeIcon;
    private Label lblTimeCaption;
    private Label lblTime;
    private Label lblFavoriteStar;

    private Panel bodySection;
    private RoundedPanel bodyIconBox;
    private Label lblBodyIcon;
    private Label lblBodyCaption;
    private RoundedPanel bodyBox;
    private Label lblBody;

    private Panel footerArea;
    private RoundedPanel btnOpenExternalLink;
    private Label lblOpenExternalLinkText;

    // 인터페이스 호환용 hidden 버튼. 실제 즐겨찾기 UI는 lblFavoriteStar.
    private Button btnFavorite;

    // 인터페이스 호환을 위해 lblTitle 필드만 유지. 실제 표시는 titleBar.TitleText.
    private Label lblTitle;

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
        scrollHost = new Panel();
        sectionStack = new FlowLayoutPanel();

        timeSection = new Panel();
        timeIconBox = new RoundedPanel();
        lblTimeIcon = new Label();
        lblTimeCaption = new Label();
        lblTime = new Label();
        lblFavoriteStar = new Label();

        bodySection = new Panel();
        bodyIconBox = new RoundedPanel();
        lblBodyIcon = new Label();
        lblBodyCaption = new Label();
        bodyBox = new RoundedPanel();
        lblBody = new Label();

        footerArea = new Panel();
        btnOpenExternalLink = new RoundedPanel();
        lblOpenExternalLinkText = new Label();

        btnFavorite = new Button();
        lblTitle = new Label();

        SuspendLayout();
        scrollHost.SuspendLayout();
        sectionStack.SuspendLayout();
        timeSection.SuspendLayout();
        bodySection.SuspendLayout();
        bodyBox.SuspendLayout();
        footerArea.SuspendLayout();

        //
        // EventDetailView
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(480, 540);
        MinimumSize = new System.Drawing.Size(360, 280);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "EventDetailView";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "일정 상세";

        //
        // scrollHost
        //
        scrollHost.Dock = System.Windows.Forms.DockStyle.Fill;
        scrollHost.BackColor = System.Drawing.Color.White;
        scrollHost.AutoScroll = true;
        scrollHost.Padding = new System.Windows.Forms.Padding(24, 16, 24, 16);
        scrollHost.Name = "scrollHost";

        //
        // sectionStack
        //
        sectionStack.Dock = System.Windows.Forms.DockStyle.Top;
        sectionStack.AutoSize = true;
        sectionStack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        sectionStack.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        sectionStack.WrapContents = false;
        sectionStack.BackColor = System.Drawing.Color.White;
        sectionStack.Name = "sectionStack";

        //
        // timeSection
        //
        timeSection.Height = 56;
        timeSection.BackColor = System.Drawing.Color.White;
        timeSection.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        timeSection.Name = "timeSection";

        timeIconBox.Location = new System.Drawing.Point(0, 8);
        timeIconBox.Size = new System.Drawing.Size(36, 36);
        timeIconBox.FillColor = System.Drawing.Color.FromArgb(254, 226, 226);
        timeIconBox.BorderSize = 0;
        timeIconBox.BorderRadius = 10;
        timeIconBox.Name = "timeIconBox";

        lblTimeIcon.Dock = System.Windows.Forms.DockStyle.Fill;
        lblTimeIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 글리프 보정: 좌하 치우침 보정용 좌 padding + 미세한 상하 보정.
        lblTimeIcon.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
        lblTimeIcon.Font = new System.Drawing.Font("Segoe UI Symbol", 13F, System.Drawing.FontStyle.Regular);
        lblTimeIcon.ForeColor = System.Drawing.Color.FromArgb(190, 24, 73);
        lblTimeIcon.BackColor = System.Drawing.Color.Transparent;
        lblTimeIcon.Text = "🕐";
        lblTimeIcon.Name = "lblTimeIcon";
        timeIconBox.Controls.Add(lblTimeIcon);

        lblTimeCaption.Location = new System.Drawing.Point(44, 6);
        lblTimeCaption.AutoSize = true;
        lblTimeCaption.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
        lblTimeCaption.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
        lblTimeCaption.BackColor = System.Drawing.Color.Transparent;
        lblTimeCaption.Text = "일시";
        lblTimeCaption.Name = "lblTimeCaption";

        lblTime.Location = new System.Drawing.Point(44, 26);
        lblTime.AutoSize = true;
        lblTime.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold);
        lblTime.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
        lblTime.BackColor = System.Drawing.Color.Transparent;
        lblTime.Text = "2026.01.01 00:00 - 00:00";
        lblTime.Name = "lblTime";

        // 즐겨찾기 별 (timeSection 우측, Anchor로 우측 따라가게)
        lblFavoriteStar.Size = new System.Drawing.Size(40, 40);
        lblFavoriteStar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        lblFavoriteStar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblFavoriteStar.Font = new System.Drawing.Font("Segoe UI Symbol", 16F, System.Drawing.FontStyle.Regular);
        lblFavoriteStar.ForeColor = System.Drawing.Color.FromArgb(250, 204, 21);
        lblFavoriteStar.BackColor = System.Drawing.Color.Transparent;
        lblFavoriteStar.Cursor = System.Windows.Forms.Cursors.Hand;
        lblFavoriteStar.Text = "☆";
        lblFavoriteStar.Name = "lblFavoriteStar";
        // 위치는 코드에서 timeSection.Width - 40, Top 8로 세팅됨 (ReflowBody).

        timeSection.Controls.Add(timeIconBox);
        timeSection.Controls.Add(lblTimeCaption);
        timeSection.Controls.Add(lblTime);
        timeSection.Controls.Add(lblFavoriteStar);

        //
        // bodySection
        //
        bodySection.AutoSize = true;
        bodySection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        bodySection.BackColor = System.Drawing.Color.White;
        bodySection.Margin = new System.Windows.Forms.Padding(0);
        bodySection.Name = "bodySection";

        bodyIconBox.Location = new System.Drawing.Point(0, 4);
        bodyIconBox.Size = new System.Drawing.Size(36, 36);
        bodyIconBox.FillColor = System.Drawing.Color.FromArgb(243, 244, 246);
        bodyIconBox.BorderSize = 0;
        bodyIconBox.BorderRadius = 10;
        bodyIconBox.Name = "bodyIconBox";

        lblBodyIcon.Dock = System.Windows.Forms.DockStyle.Fill;
        lblBodyIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblBodyIcon.Padding = new System.Windows.Forms.Padding(4, 0, 0, 4);
        lblBodyIcon.Font = new System.Drawing.Font("Segoe UI Symbol", 13F, System.Drawing.FontStyle.Regular);
        lblBodyIcon.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
        lblBodyIcon.BackColor = System.Drawing.Color.Transparent;
        lblBodyIcon.Text = "≡";
        lblBodyIcon.Name = "lblBodyIcon";
        bodyIconBox.Controls.Add(lblBodyIcon);

        lblBodyCaption.Location = new System.Drawing.Point(44, 10);
        lblBodyCaption.AutoSize = true;
        lblBodyCaption.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
        lblBodyCaption.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
        lblBodyCaption.BackColor = System.Drawing.Color.Transparent;
        lblBodyCaption.Text = "상세 내용 (원문)";
        lblBodyCaption.Name = "lblBodyCaption";

        bodyBox.Location = new System.Drawing.Point(0, 52);
        bodyBox.FillColor = System.Drawing.Color.FromArgb(249, 250, 251);
        bodyBox.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
        bodyBox.BorderSize = 1;
        bodyBox.BorderRadius = 12;
        bodyBox.Padding = new System.Windows.Forms.Padding(14);
        bodyBox.AutoSize = true;
        bodyBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        bodyBox.Name = "bodyBox";

        lblBody.Dock = System.Windows.Forms.DockStyle.Top;
        lblBody.AutoSize = true;
        lblBody.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Regular);
        lblBody.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
        lblBody.BackColor = System.Drawing.Color.Transparent;
        lblBody.TextAlign = System.Drawing.ContentAlignment.TopLeft;
        lblBody.Text = "상세 내용";
        lblBody.Name = "lblBody";

        bodyBox.Controls.Add(lblBody);
        bodySection.Controls.Add(bodyIconBox);
        bodySection.Controls.Add(lblBodyCaption);
        bodySection.Controls.Add(bodyBox);

        sectionStack.Controls.Add(timeSection);
        sectionStack.Controls.Add(bodySection);

        scrollHost.Controls.Add(sectionStack);

        //
        // footerArea
        //
        footerArea.Dock = System.Windows.Forms.DockStyle.Bottom;
        footerArea.Height = 70;
        footerArea.BackColor = System.Drawing.Color.White;
        footerArea.Padding = new System.Windows.Forms.Padding(20, 12, 20, 18);
        footerArea.Name = "footerArea";

        btnOpenExternalLink.Dock = System.Windows.Forms.DockStyle.Fill;
        btnOpenExternalLink.FillColor = System.Drawing.Color.FromArgb(136, 19, 55);
        btnOpenExternalLink.BorderSize = 0;
        btnOpenExternalLink.BorderRadius = 14;
        btnOpenExternalLink.Cursor = System.Windows.Forms.Cursors.Hand;
        btnOpenExternalLink.Name = "btnOpenExternalLink";

        lblOpenExternalLinkText.Dock = System.Windows.Forms.DockStyle.Fill;
        lblOpenExternalLinkText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblOpenExternalLinkText.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold);
        lblOpenExternalLinkText.ForeColor = System.Drawing.Color.White;
        lblOpenExternalLinkText.BackColor = System.Drawing.Color.Transparent;
        lblOpenExternalLinkText.Cursor = System.Windows.Forms.Cursors.Hand;
        lblOpenExternalLinkText.Text = "외부 링크 열기";
        lblOpenExternalLinkText.Name = "lblOpenExternalLinkText";

        btnOpenExternalLink.Controls.Add(lblOpenExternalLinkText);

        footerArea.Controls.Add(btnOpenExternalLink);

        // hidden
        btnFavorite.Visible = false;
        btnFavorite.Name = "btnFavorite";
        lblTitle.Visible = false;
        lblTitle.Name = "lblTitle";

        Controls.Add(scrollHost);
        Controls.Add(footerArea);
        // titleBar는 코드에서 Add하고 BringToFront.
        Controls.Add(btnFavorite);
        Controls.Add(lblTitle);

        footerArea.ResumeLayout(false);
        bodyBox.ResumeLayout(false);
        bodySection.ResumeLayout(false);
        timeSection.ResumeLayout(false);
        sectionStack.ResumeLayout(false);
        scrollHost.ResumeLayout(false);
        ResumeLayout(false);
    }
}
