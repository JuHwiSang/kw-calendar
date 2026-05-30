using KW_Calendar.Native;
using KW_Calendar.Presenters;
using KW_Calendar.Services;
using KW_Calendar.Views;
using Microsoft.Extensions.Configuration;

namespace KW_Calendar
{
    internal static class Program
    {
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

            var view = new CalendarView();

            // TODO: CalendarView가 ICalendarView를 구현하면 Presenter를 배선하고,
            //       아래의 직접 view.Show() 호출도 presenter.RequestOpen()으로 교체.
            // var presenter = new CalendarPresenter(view, events, cats, sync);
            // presenter.Initialize();
            // instance.OpenRequested += (_, _) => presenter.RequestOpen();

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
