using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    /// <summary>
    /// Form을 "항상 다른 모든 창의 아래, 그러나 바탕화면 위" 레이어에 고정시킨다.
    /// VueMinder, Rainmeter 등이 사용하는 표준 방식.
    /// </summary>
    public static class DesktopAnchor
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public static readonly IntPtr HWND_BOTTOM = new(1);

        /// <summary>
        /// 부착 시점에 호출. 윈도우 핸들이 만들어진 후여야 한다.
        /// </summary>
        public static void Anchor(IntPtr handle)
        {
            var ex = GetWindowLong(handle, GWL_EXSTYLE);
            SetWindowLong(handle, GWL_EXSTYLE, ex | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
            SinkToBottom(handle);
        }

        public static void SinkToBottom(IntPtr handle)
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

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        private static long GetWindowLong(IntPtr hWnd, int nIndex)
            => IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, nIndex).ToInt64()
                : GetWindowLong32(hWnd, nIndex).ToInt64();

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong)
            => IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, new IntPtr(dwNewLong))
                : SetWindowLong32(hWnd, nIndex, new IntPtr(dwNewLong));

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
