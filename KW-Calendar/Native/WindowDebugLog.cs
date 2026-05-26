using System.IO;

namespace KW_Calendar.Native
{
    /// <summary>
    /// Win+D 디버깅용 임시 로거. 위젯에 일어나는 윈도우 메시지/상태 변화를
    /// 디스크에 append한다. 문제 진단 후 제거 예정.
    /// 출력 경로: %LOCALAPPDATA%\KW-Calendar\widget-debug.log
    /// </summary>
    public static class WindowDebugLog
    {
        private static readonly object Gate = new();
        private static readonly string Path = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KW-Calendar", "widget-debug.log");

        static WindowDebugLog()
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
                File.WriteAllText(Path, $"=== session start {DateTime.Now:HH:mm:ss.fff} ===\n");
            }
            catch { }
        }

        public static void Log(string line)
        {
            try
            {
                lock (Gate)
                {
                    File.AppendAllText(Path, $"{DateTime.Now:HH:mm:ss.fff} {line}\n");
                }
            }
            catch { }
        }

        public static string MsgName(int msg) => msg switch
        {
            0x0018 => "WM_SHOWWINDOW",
            0x001C => "WM_ACTIVATEAPP",
            0x0006 => "WM_ACTIVATE",
            0x0086 => "WM_NCACTIVATE",
            0x0046 => "WM_WINDOWPOSCHANGING",
            0x0047 => "WM_WINDOWPOSCHANGED",
            0x0005 => "WM_SIZE",
            0x0112 => "WM_SYSCOMMAND",
            0x0231 => "WM_ENTERSIZEMOVE",
            0x0232 => "WM_EXITSIZEMOVE",
            0x0020 => "WM_SETCURSOR",
            0x0024 => "WM_GETMINMAXINFO",
            0x007F => "WM_GETICON",
            0x0281 => "WM_IME_SETCONTEXT",
            _ => $"0x{msg:X4}",
        };

        public static string PosFlags(uint flags)
        {
            var parts = new List<string>();
            if ((flags & 0x0001) != 0) parts.Add("NOSIZE");
            if ((flags & 0x0002) != 0) parts.Add("NOMOVE");
            if ((flags & 0x0004) != 0) parts.Add("NOZORDER");
            if ((flags & 0x0008) != 0) parts.Add("NOREDRAW");
            if ((flags & 0x0010) != 0) parts.Add("NOACTIVATE");
            if ((flags & 0x0020) != 0) parts.Add("FRAMECHANGED");
            if ((flags & 0x0040) != 0) parts.Add("SHOWWINDOW");
            if ((flags & 0x0080) != 0) parts.Add("HIDEWINDOW");
            if ((flags & 0x0100) != 0) parts.Add("NOCOPYBITS");
            if ((flags & 0x0200) != 0) parts.Add("NOOWNERZORDER");
            if ((flags & 0x0400) != 0) parts.Add("NOSENDCHANGING");
            return parts.Count == 0 ? "0" : string.Join("|", parts);
        }
    }
}
