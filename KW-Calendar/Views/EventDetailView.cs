using System.ComponentModel;
using KW_Calendar.Models;
using KW_Calendar.Native;

namespace KW_Calendar.Views;

public partial class EventDetailView : Form, IEventDetailView
{
    private Event currentEvent = new Event();
    private CustomTitleBar titleBar;

    private const int MaxContentHeight = 700;
    private const int MinContentHeight = 200;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Event CurrentEvent
    {
        get => currentEvent;
        set
        {
            currentEvent = value ?? new Event();
            RenderEvent();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsFavorited
    {
        get => currentEvent.IsFavorited;
        set
        {
            currentEvent.IsFavorited = value;
            UpdateFavoriteState();
        }
    }

    public event EventHandler<int> FavoriteToggleRequested = delegate { };
    public event EventHandler OpenExternalLinkRequested = delegate { };
    public event EventHandler ViewClosed = delegate { };

    public EventDetailView()
    {
        InitializeComponent();

        titleBar = new CustomTitleBar
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = System.Drawing.Color.White,
            TitleText = "일정 상세",
            ShowMaximize = false,
            ShowMinimize = false
        };
        Controls.Add(titleBar);
        // Dock 순서를 올바르게 잡는다.
        // 의도: titleBar(Top) → footerArea(Bottom) → scrollHost(Fill)
        // Z-order는 add 순이고 Dock 처리는 그 역순. titleBar가 가장 마지막에 add되어
        // 가장 먼저 dock 처리되며 Top 36 차지. scrollHost가 남은 공간을 채움.
        PerformLayout();

        lblFavoriteStar.Click += (s, e) => FavoriteToggleRequested(this, currentEvent.Id);
        btnFavorite.Click += BtnFavorite_Click;
        btnOpenExternalLink.Click += BtnOpenExternalLink_Click;
        lblOpenExternalLinkText.Click += BtnOpenExternalLink_Click;
        FormClosed += EventDetailView_FormClosed;

        scrollHost.SizeChanged += (s, e) => ReflowBody();
        sectionStack.SizeChanged += (s, e) => AdjustFormHeight();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowHelpers.ApplyRoundedCorners(Handle);
    }

    private void BtnFavorite_Click(object? sender, EventArgs e)
    {
        FavoriteToggleRequested(this, currentEvent.Id);
    }

    private void BtnOpenExternalLink_Click(object? sender, EventArgs e)
    {
        OpenExternalLinkRequested(this, EventArgs.Empty);
    }

    private void EventDetailView_FormClosed(object? sender, FormClosedEventArgs e)
    {
        ViewClosed(this, EventArgs.Empty);
    }

    private void RenderEvent()
    {
        if (lblTime == null) return;

        titleBar.TitleText = string.IsNullOrWhiteSpace(currentEvent.Title) ? "일정 상세" : currentEvent.Title;
        lblTime.Text = FormatTimeRange(currentEvent);
        lblBody.Text = string.IsNullOrWhiteSpace(currentEvent.Body) ? "(내용 없음)" : currentEvent.Body!;

        bool hasLink = !string.IsNullOrWhiteSpace(currentEvent.ExternalLink);
        footerArea.Visible = hasLink;

        ReflowBody();
        AdjustFormHeight();
        UpdateFavoriteState();
    }

    private static string FormatTimeRange(Event ev)
    {
        if (ev.StartDt == default && ev.EndDt == null)
            return "-";

        if (ev.EndDt == null)
            return $"{ev.StartDt:yyyy.MM.dd HH:mm}";

        DateTime endDt = ev.EndDt.Value;

        if (ev.StartDt.Date == endDt.Date)
            return $"{ev.StartDt:yyyy.MM.dd HH:mm} - {endDt:HH:mm}";

        return $"{ev.StartDt:yyyy.MM.dd HH:mm} - {endDt:yyyy.MM.dd HH:mm}";
    }

    private bool _reflowing;

    private void ReflowBody()
    {
        if (_reflowing) return;
        if (scrollHost == null || sectionStack == null) return;
        if (scrollHost.ClientSize.Width <= 0) return;

        _reflowing = true;
        try
        {
            int hostInner = scrollHost.ClientSize.Width - scrollHost.Padding.Horizontal;
            if (hostInner <= 0) return;

            sectionStack.Width = hostInner;

            int sectionWidth = sectionStack.ClientSize.Width;
            timeSection.Width = sectionWidth;

            // 별 우측 정렬: timeSection 폭 - 별 폭
            lblFavoriteStar.Left = sectionWidth - lblFavoriteStar.Width;
            lblFavoriteStar.Top = (timeSection.Height - lblFavoriteStar.Height) / 2;

            bodySection.MinimumSize = new System.Drawing.Size(sectionWidth, 0);
            bodySection.MaximumSize = new System.Drawing.Size(sectionWidth, 0);

            int boxWidth = sectionWidth;
            bodyBox.MinimumSize = new System.Drawing.Size(boxWidth, 80);
            bodyBox.MaximumSize = new System.Drawing.Size(boxWidth, 0);
            bodyBox.Width = boxWidth;

            // 보정값: FixedSingle border(2) + 스크롤바가 갑자기 등장할 때를 위한 여유(20).
            int textWidth = boxWidth - bodyBox.Padding.Horizontal - 22;
            lblBody.MaximumSize = new System.Drawing.Size(Math.Max(0, textWidth), 0);
        }
        finally
        {
            _reflowing = false;
        }
    }

    private bool _adjustingHeight;

    private void AdjustFormHeight()
    {
        if (_adjustingHeight) return;
        if (sectionStack == null || titleBar == null) return;
        if (sectionStack.PreferredSize.Height <= 0) return;

        _adjustingHeight = true;
        try
        {
            int contentNeeded = sectionStack.PreferredSize.Height
                + scrollHost.Padding.Vertical;

            int clipped = Math.Clamp(contentNeeded, MinContentHeight, MaxContentHeight);

            int targetClientHeight = titleBar.Height
                + clipped
                + (footerArea.Visible ? footerArea.Height : 0);

            if (ClientSize.Height != targetClientHeight)
            {
                ClientSize = new System.Drawing.Size(ClientSize.Width, targetClientHeight);
            }
        }
        finally
        {
            _adjustingHeight = false;
        }
    }

    private void UpdateFavoriteState()
    {
        if (lblFavoriteStar == null)
            return;

        bool fav = currentEvent.IsFavorited;
        lblFavoriteStar.Text = fav ? "★" : "☆";
        lblFavoriteStar.ForeColor = fav
            ? System.Drawing.Color.FromArgb(250, 204, 21)
            : System.Drawing.Color.FromArgb(203, 213, 225);

        if (btnFavorite != null)
            btnFavorite.Text = fav ? "★" : "☆";
    }
}
