using KW_Calendar.Native;
using KW_Calendar.Presenters;
using KW_Calendar.Services;
using KW_Calendar.Views;
using System.IO;
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
            using var instance = SingleInstance.Acquire();
            if (!instance.IsFirstInstance)
            {
                SingleInstance.NotifyExisting();
                return;
            }

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

            var view = new CalendarView();
            var presenter = new CalendarPresenter(view, events, cats, sync);
            instance.OpenRequested += (_, _) => presenter.RequestOpen();
            presenter.Initialize();

            // 메인 창이 떠서 활성화된 뒤에 위젯을 만들어 띄운다. 위젯이 핸들 생성 시점에
            // HWND_BOTTOM으로 가라앉으면서 같은 thread의 메인 창까지 같이 끌어내리는
            // 시작 시 z-order 부작용을 피하기 위함.
            view.Shown += (_, _) =>
            {
                var widget = new CalendarWidgetView();
                var widgetPresenter = new CalendarPresenter(widget, events, cats, sync);
                widgetPresenter.Initialize();
                widget.Show();
            };

            using var tray = CreateTrayIcon(view);

            instance.OpenRequested += (_, _) =>
            {
                if (view.IsHandleCreated)
                    view.BeginInvoke(() => view.Show());
            };
            view.HandleCreated += (_, _) => instance.StartListening();

            Application.Run(view);
        }

        private static NotifyIcon CreateTrayIcon(Form view)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("열기", null, (_, _) => view.Show());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("종료", null, (_, _) => Application.Exit());

            var icon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "KW-Calendar",
                Visible = true,
                ContextMenuStrip = menu,
            };
            icon.DoubleClick += (_, _) => view.Show();
            return icon;
        }
    }
}
