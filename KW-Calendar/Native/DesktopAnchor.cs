using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    /// <summary>
    /// Form을 "항상 다른 모든 창의 아래, 그러나 바탕화면 위" 레이어에 고정시킨다.
    /// VueMinder, Rainmeter 등이 사용하는 표준 방식.
    /// </summary>
    public static class DesktopAnchor
    {
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public static readonly IntPtr HWND_BOTTOM = new(1);

        /// <summary>
        /// 부착 시점에 호출. 윈도우 핸들이 만들어진 후여야 한다.
        /// EX_STYLE(WS_EX_TOOLWINDOW|WS_EX_NOACTIVATE) 설정은 DesktopWidgetForm.CreateParams가
        /// 담당하므로 여기서는 z순서만 맨 아래로 끌어내린다.
        /// </summary>
        public static void Anchor(IntPtr handle)
        {
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOACTIVATE = 0x0010;
            SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }
    }
}
