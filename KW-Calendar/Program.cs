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

            // TODO: CalendarView가 ICalendarView를 구현하면 메인 창에도 Presenter 연결
            var mainView = new CalendarView();

            var widget = new CalendarWidgetView();
            var widgetPresenter = new CalendarPresenter(widget, events, cats, sync);
            widgetPresenter.Initialize();
            widget.Show();

            Application.Run(mainView);
        }
    }
}
