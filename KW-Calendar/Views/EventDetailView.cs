using KW_Calendar.Models;

namespace KW_Calendar.Views;

public partial class EventDetailView : Form, IEventDetailView
{
    private Event currentEvent = new Event();

    public Event CurrentEvent
    {
        get => currentEvent;
        set
        {
            currentEvent = value;
            RenderEvent();
        }
    }

    public bool IsFavorited
    {
        get => currentEvent.IsFavorited;
        set
        {
            currentEvent.IsFavorited = value;
            UpdateFavoriteState();
        }
    }

    public event EventHandler<int>? FavoriteToggleRequested;
    public event EventHandler? OpenExternalLinkRequested;
    public event EventHandler? ViewClosed;

    public EventDetailView()
    {
        InitializeComponent();

        btnFavorite.Click += BtnFavorite_Click;
        btnOpenExternalLink.Click += BtnOpenExternalLink_Click;

        FormClosed += EventDetailView_FormClosed;
    }

    private void BtnFavorite_Click(object? sender, EventArgs e)
    {
        FavoriteToggleRequested?.Invoke(this, currentEvent.Id);
    }

    private void BtnOpenExternalLink_Click(object? sender, EventArgs e)
    {
        OpenExternalLinkRequested?.Invoke(this, EventArgs.Empty);
    }

    private void EventDetailView_FormClosed(object? sender, FormClosedEventArgs e)
    {
        ViewClosed?.Invoke(this, EventArgs.Empty);
    }

    private void RenderEvent()
    {
        lblTitle.Text = currentEvent.Title;
        lblTime.Text = $"{currentEvent.StartDt:yyyy.MM.dd HH:mm} - {currentEvent.EndDt:HH:mm}";
        lblBody.Text = currentEvent.Body ?? "";

        btnOpenExternalLink.Visible =
            !string.IsNullOrWhiteSpace(currentEvent.ExternalLink);

        UpdateFavoriteState();
    }

    private void UpdateFavoriteState()
    {
        btnFavorite.Text = currentEvent.IsFavorited ? "★" : "☆";
    }
}
