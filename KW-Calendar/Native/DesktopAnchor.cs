using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    /// <summary>
    /// Form의 Z순서를 "다른 모든 창의 아래, 그러나 바탕화면 위" 레이어에 고정하기 위한 헬퍼.
    /// VueMinder, Rainmeter 등이 사용하는 표준 방식.
    ///
    /// 동작 원리는 두 가지 조합으로 이루어진다:
    /// 1) 부착 시점에 한 번 HWND_BOTTOM으로 Z순서 강하 (이 클래스의 Anchor 호출)
    /// 2) WndProc에서 WM_WINDOWPOSCHANGING을 가로채 hwndInsertAfter를 매번 HWND_BOTTOM으로
    ///    덮어써서, 다른 창이 활성화되어도 위로 올라오지 못하게 한다 (Form 측에서 처리)
    ///
    /// SetParent로 Progman/WorkerW에 부착하는 방식과 달리, 일반 top-level 윈도우로 남기
    /// 때문에 raised desktop(Win11)의 layered SHELLDLL_DefView가 마우스 hit-test를 가로채는
    /// 문제가 없어 클릭이 정상 동작한다.
    ///
    /// (참고: 작업표시줄 제외/포커스 동작/Win+D 통과는 Form 측에서 윈도우 스타일 비트로 처리.
    ///  CalendarWidgetView.CreateParams 참고.)
    /// </summary>
    public static class DesktopAnchor
    {
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public static readonly IntPtr HWND_BOTTOM = new(1);

        /// <summary>
        /// 부착 시점에 한 번 호출. 윈도우 핸들이 만들어진 후여야 한다.
        /// 이후 Z순서 유지는 WndProc에서 WM_WINDOWPOSCHANGING을 가로채 처리한다.
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

        /// <summary>
        /// WM_WINDOWPOSCHANGING의 lParam 으로 들어오는 WINDOWPOS 구조체.
        /// hwndInsertAfter 필드를 HWND_BOTTOM 으로 덮어쓰면 Z순서 상승이 막힌다.
        /// </summary>
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
