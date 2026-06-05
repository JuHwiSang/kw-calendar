using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    /// <summary>
    /// 시작 폴더에 .lnk를 만들어 Windows 로그인 시 자동 실행되게 한다.
    ///
    /// 현재는 어디서도 호출하지 않는다. 자동 실행을 켜고 끄는 토글 UI가 없는
    /// 상태에서 무조건 Enable()을 부르면, 비개발자 사용자가 자동 실행을 끌
    /// 방법이 없어 매번 부팅할 때마다 앱이 뜬다. 토글 UI(설정 화면이나
    /// 트레이 메뉴 항목)가 붙기 전까지 보류.
    /// </summary>
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
