using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    internal static class StartupShortcut
    {
        private const string ShortcutName = "KW-Calendar.lnk";

        public static bool IsEnabled => File.Exists(ShortcutPath);

        public static void Enable()
        {
            var exePath = Environment.ProcessPath
                ?? throw new InvalidOperationException("실행 파일 경로를 확인할 수 없습니다.");

            var shellType = Type.GetTypeFromProgID("WScript.Shell")
                ?? throw new InvalidOperationException("WScript.Shell COM을 사용할 수 없습니다.");

            dynamic shell = Activator.CreateInstance(shellType)!;
            try
            {
                dynamic link = shell.CreateShortcut(ShortcutPath);
                try
                {
                    link.TargetPath = exePath;
                    link.WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty;
                    link.WindowStyle = 7; // Minimized
                    link.Description = "KW-Calendar";
                    link.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(link);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        public static void Disable()
        {
            if (File.Exists(ShortcutPath))
            {
                File.Delete(ShortcutPath);
            }
        }

        private static string ShortcutPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ShortcutName);
    }
}
