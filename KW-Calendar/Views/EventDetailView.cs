using System.ComponentModel;
using KW_Calendar.Models;
using KW_Calendar.Native;

namespace KW_Calendar.Views;

public partial class EventDetailView : Form, IEventDetailView
{
    private Event currentEvent = new Event();

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

        var titleBar = new CustomTitleBar
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = System.Drawing.Color.White,
            TitleText = "일정 상세",
            ShowMaximize = false,
            ShowMinimize = false
        };
        Controls.Add(titleBar);
        titleBar.BringToFront();

        btnFavorite.Click += BtnFavorite_Click;
        btnOpenExternalLink.Click += BtnOpenExternalLink_Click;
        FormClosed += EventDetailView_FormClosed;
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
        if (lblTitle == null)
            return;

        lblTitle.Text = currentEvent.Title;
        lblTime.Text = $"{currentEvent.StartDt:yyyy.MM.dd HH:mm} - {currentEvent.EndDt:HH:mm}";
        lblBody.Text = currentEvent.Body ?? "";

        btnOpenExternalLink.Visible =
            !string.IsNullOrWhiteSpace(currentEvent.ExternalLink);

        UpdateFavoriteState();
    }

    private void UpdateFavoriteState()
    {
        if (btnFavorite == null)
            return;

        btnFavorite.Text = currentEvent.IsFavorited ? "★" : "☆";
    }
}
