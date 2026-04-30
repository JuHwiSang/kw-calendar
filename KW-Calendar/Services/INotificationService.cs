namespace KW_Calendar.Services;

public interface INotificationService
{
    // 즐겨찾기 이벤트 중 NoticeDt가 지난 미발송 알림을 Windows 토스트로 전송
    Task CheckAndSendPendingNotificationsAsync(CancellationToken ct = default);
}
