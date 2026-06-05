using KW_Calendar.Models;
using CommunityToolkit.WinUI.Notifications;

namespace KW_Calendar.Services;

public class NotificationService : INotificationService
{
    private readonly ILocalDbService _localDb;
    private readonly Func<DateTime> _nowProvider;

    public NotificationService(ILocalDbService localDb, Func<DateTime>? nowProvider = null)
    {
        _localDb = localDb;
        _nowProvider = nowProvider ?? (() => DateTime.Now);
    }

    public async Task CheckAndSendPendingNotificationsAsync(CancellationToken ct = default)
    {
        var favoritedEvents = await _localDb.GetFavoritedEventsAsync(ct);
        var now = _nowProvider();

        foreach (var ev in favoritedEvents)
        {
            var oneDayBeforeTime = ev.StartDt.Date.AddDays(-1).AddHours(9);
            var sameDayTime = ev.StartDt.Date.AddHours(9);

            if (now >= oneDayBeforeTime && !ev.IsOneDayBeforeNotified)
            {
                TrySendToast(ev, isDayBefore: true);
                await _localDb.MarkEventNotificationsAsync(ev.Id, oneDayBefore: true, sameDay: null, ct);
            }

            if (now >= sameDayTime && !ev.IsSameDayNotified)
            {
                TrySendToast(ev, isDayBefore: false);
                await _localDb.MarkEventNotificationsAsync(ev.Id, oneDayBefore: null, sameDay: true, ct);
            }
        }
    }

    internal static void TrySendToast(Event ev, bool isDayBefore)
    {
        try
        {
            var when = isDayBefore ? $"내일 ({ev.StartDt:M/d})" : $"오늘 ({ev.StartDt:M/d})";
            var builder = new ToastContentBuilder()
                .AddText(ev.Title)
                .AddText($"{when} 일정이 있습니다.");

            if (!string.IsNullOrEmpty(ev.ExternalLink))
                builder.SetProtocolActivation(new Uri(ev.ExternalLink));

            builder.Show();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[NotificationService] Notification failed: {ex.Message}");
        }
    }
}
