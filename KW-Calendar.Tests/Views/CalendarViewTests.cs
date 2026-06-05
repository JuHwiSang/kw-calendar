using KW_Calendar.Views;

namespace KW_Calendar.Tests.Views;

public class CalendarViewTests
{
    // protected OnFormClosing을 테스트에서 직접 부르기 위한 얇은 래퍼.
    // 실제 동작(취소 + Hide)을 그대로 호출해 행동을 검증한다.
    private sealed class TestableCalendarView : CalendarView
    {
        public void InvokeFormClosing(FormClosingEventArgs e) => OnFormClosing(e);
    }

    [Theory]
    [InlineData(CloseReason.UserClosing, true)]              // X / Alt+F4 → 트레이로 숨김
    [InlineData(CloseReason.ApplicationExitCall, false)]     // 트레이 "종료" → 진짜 종료
    [InlineData(CloseReason.WindowsShutDown, false)]         // OS 종료 → 막지 않음
    [InlineData(CloseReason.TaskManagerClosing, false)]      // 작업관리자 → 막지 않음
    [InlineData(CloseReason.FormOwnerClosing, false)]
    [InlineData(CloseReason.MdiFormClosing, false)]
    [InlineData(CloseReason.None, false)]
    public void OnFormClosing_CancelsOnlyForUserClosing(CloseReason reason, bool shouldCancel)
    {
        using var view = new TestableCalendarView();
        var args = new FormClosingEventArgs(reason, cancel: false);

        view.InvokeFormClosing(args);

        Assert.Equal(shouldCancel, args.Cancel);
    }

    [Fact]
    public void OnFormClosing_UserClosing_HidesForm()
    {
        using var view = new TestableCalendarView();
        view.Show();
        Assert.True(view.Visible);

        view.InvokeFormClosing(new FormClosingEventArgs(CloseReason.UserClosing, cancel: false));

        Assert.False(view.Visible);
    }
}
