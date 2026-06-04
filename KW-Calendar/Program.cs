using System.IO;
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
            var presenter = new CalendarPresenter(view, events, cats, sync);
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

            Application.Run(view);
        }
    }
}
