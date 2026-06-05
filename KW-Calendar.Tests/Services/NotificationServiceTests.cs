using KW_Calendar.Models;
using KW_Calendar.Services;
using NSubstitute;

namespace KW_Calendar.Tests.Services;

public class NotificationServiceTests
{
    private readonly ILocalDbService _localDb;

    // 고정 기준 시각: 2026-06-15 10:00 (09:00 이후)
    private static readonly DateTime FakeNow = new(2026, 6, 15, 10, 0, 0);

    public NotificationServiceTests()
    {
        _localDb = Substitute.For<ILocalDbService>();
    }

    private NotificationService CreateSut(DateTime? now = null)
        => new(_localDb, () => now ?? FakeNow);

    [Fact]
    public async Task Check_SendsOneDayBeforeNotification_WhenDayBeforeNineAMHasPassed()
    {
        // D-1 09:00 = 2026-06-15 09:00 → FakeNow(10:00)보다 이전이므로 조건 충족
        var ev = new Event { Id = 1, Title = "Test", StartDt = new DateTime(2026, 6, 16), IsOneDayBeforeNotified = false };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut().CheckAndSendPendingNotificationsAsync();

        await _localDb.Received(1).MarkEventNotificationsAsync(1, oneDayBefore: true, sameDay: null);
    }

    [Fact]
    public async Task Check_SendsSameDayNotification_WhenSameDayNineAMHasPassed()
    {
        // 당일 09:00 = 2026-06-15 09:00 → FakeNow(10:00)보다 이전이므로 조건 충족
        var ev = new Event { Id = 2, Title = "Test", StartDt = new DateTime(2026, 6, 15, 18, 0, 0), IsSameDayNotified = false };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut().CheckAndSendPendingNotificationsAsync();

        await _localDb.Received(1).MarkEventNotificationsAsync(2, oneDayBefore: null, sameDay: true);
    }

    [Fact]
    public async Task Check_SendsBothNotifications_WhenBothTimesHavePassedAndNeitherSent()
    {
        // FakeNow 기준 D-1 09:00, 당일 09:00 모두 경과
        var ev = new Event { Id = 3, Title = "Test", StartDt = new DateTime(2026, 6, 15, 18, 0, 0), IsOneDayBeforeNotified = false, IsSameDayNotified = false };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut().CheckAndSendPendingNotificationsAsync();

        await _localDb.Received(1).MarkEventNotificationsAsync(3, oneDayBefore: true, sameDay: null);
        await _localDb.Received(1).MarkEventNotificationsAsync(3, oneDayBefore: null, sameDay: true);
    }

    [Fact]
    public async Task Check_SkipsOneDayBeforeNotification_WhenAlreadySent()
    {
        var ev = new Event { Id = 4, Title = "Test", StartDt = new DateTime(2026, 6, 16), IsOneDayBeforeNotified = true };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut().CheckAndSendPendingNotificationsAsync();

        await _localDb.DidNotReceive().MarkEventNotificationsAsync(4, oneDayBefore: true, sameDay: Arg.Any<bool?>());
    }

    [Fact]
    public async Task Check_SkipsSameDayNotification_WhenAlreadySent()
    {
        var ev = new Event { Id = 5, Title = "Test", StartDt = new DateTime(2026, 6, 15, 18, 0, 0), IsSameDayNotified = true };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut().CheckAndSendPendingNotificationsAsync();

        await _localDb.DidNotReceive().MarkEventNotificationsAsync(5, oneDayBefore: Arg.Any<bool?>(), sameDay: true);
    }

    [Fact]
    public async Task Check_DoesNotSend_WhenNineAMHasNotPassedYet()
    {
        // 08:00 기준 → D-1 09:00 미도달
        var ev = new Event { Id = 6, Title = "Test", StartDt = new DateTime(2026, 6, 16), IsOneDayBeforeNotified = false };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut(now: new DateTime(2026, 6, 15, 8, 0, 0)).CheckAndSendPendingNotificationsAsync();

        await _localDb.DidNotReceive().MarkEventNotificationsAsync(Arg.Any<int>(), Arg.Any<bool?>(), Arg.Any<bool?>());
    }

    [Fact]
    public async Task Check_SendsNotification_WhenEventHasExternalLink()
    {
        // ExternalLink가 있는 이벤트도 정상적으로 알림 발송 흐름이 완료되어야 함
        var ev = new Event { Id = 7, Title = "Test", StartDt = new DateTime(2026, 6, 16), ExternalLink = "https://www.kw.ac.kr", IsOneDayBeforeNotified = false };
        _localDb.GetFavoritedEventsAsync().Returns(new List<Event> { ev });

        await CreateSut().CheckAndSendPendingNotificationsAsync();

        await _localDb.Received(1).MarkEventNotificationsAsync(7, oneDayBefore: true, sameDay: null);
    }
}
