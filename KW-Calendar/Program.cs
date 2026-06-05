using KW_Calendar.Presenters;
using KW_Calendar.Services;
using KW_Calendar.Views;
using Microsoft.Extensions.Configuration;
using Timer = System.Threading.Timer;

namespace KW_Calendar
{
    internal static class Program
    {
        private static Timer? _notificationTimer;

        private static TimeSpan GetDelayUntilNextNineAM()
        {
            var now = DateTime.Now;
            var nineToday = now.Date.AddHours(9);
            var next = now < nineToday ? nineToday : nineToday.AddDays(1);
            return next - now;
        }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var dbPath = Environment.ExpandEnvironmentVariables(config["LocalDb:Path"]!);
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            var localDb = new LocalDbService(dbPath);
            var supabase = new SupabaseService(config);
            var sync = new SyncService(supabase, localDb);
            var events = new EventService(localDb);
            var cats = new CategoryService(localDb);
            var notifications = new NotificationService(localDb);

            // 앱 시작 시 누락된 알림 일괄 발송 (Application.Run 전이므로 동기 실행)
            notifications.CheckAndSendPendingNotificationsAsync().GetAwaiter().GetResult();

            // 다음 09:00부터 24시간 간격으로 알림 체크 (GC 방지를 위해 static 보관)
            // TODO: 절전/초절전 모드에서는 타이머가 정지할 수 있어 알림이 누락될 수 있다.
            //       개선 방법: min(9:00까지 딜레이, 30분) 단위로 쪼개고 Timer.Change로 재초기화하는 방식 검토.
            _notificationTimer = new Timer(
                _ => notifications.CheckAndSendPendingNotificationsAsync().GetAwaiter().GetResult(),
                null, GetDelayUntilNextNineAM(), TimeSpan.FromHours(24));

            // TODO: CalendarView가 ICalendarView를 구현하면 아래 주석 해제
            var view = new CalendarView();
            var presenter = new CalendarPresenter(view, events, cats, sync);
            presenter.Initialize();

            Application.Run(view);
        }
    }
}
