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

            // TODO: 서비스 구현체 완성 후 수동 조립
            // var localDb   = new LocalDbService(config);
            // var sync      = new SyncService(config, localDb);
            // var events    = new EventService(localDb);
            // var cats      = new CategoryService(localDb);
            // var notif     = new NotificationService(localDb);
            // var presenter = new CalendarPresenter(view, events, cats, sync);

            Application.Run(new CalendarView());
        }
    }
}
