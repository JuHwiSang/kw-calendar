using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    /// <summary>
    /// 바탕화면 위에 떠 있는 위젯의 공용 동작을 담는 베이스 Form.
    ///
    /// === "바탕화면 위, 다른 모든 창 아래" 트릭 ===
    /// 1) 일반 top-level 윈도우로 두고, WM_WINDOWPOSCHANGING에서 z-order 변경 의도가
    ///    있을 때 hwndInsertAfter를 HWND_BOTTOM으로 강제. (Rainmeter, VueMinder 방식)
    /// 2) WS_EX_TOOLWINDOW로 작업표시줄/Alt+Tab에서 제외, WS_EX_NOACTIVATE로
    ///    클릭해도 포커스를 가져가지 않게 한다.
    ///
    /// === Win+D / "바탕화면 보기" 대응 ===
    /// Win11 24H2부터 Shell이 Show Desktop 시 일반 창을 cloak/숨김 처리하는
    /// 메커니즘이 강화되어 WS_MINIMIZEBOX 비트 트릭만으론 부족하고, cloak 상태
    /// 변경 이벤트(WM_CLOAKED_STATE_CHANGED)도 우리 위젯에 신뢰성 있게 안 온다.
    /// → 500ms Timer로 cloak 상태를 polling하다가 cloak되면 강제 uncloak.
    /// </summary>
    public class DesktopWidgetForm : Form
    {
        protected void BeginSystemDrag()
        {
            // 호출자(컨트롤)가 mouse down으로 캡처를 잡은 상태이므로 먼저 캡처 해제.
            ReleaseCapture();
            // 시스템 드래그 모드 진입. Windows가 마우스 이동/릴리즈를 알아서 처리한다.
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int HTCAPTION = 2;
            SendMessage(Handle, WM_NCLBUTTONDOWN, new IntPtr(HTCAPTION), IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        // --- 바탕화면 앵커링 윈도우 스타일 ---
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_NOACTIVATE = 0x08000000;

                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                cp.Style &= ~WS_MINIMIZEBOX;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DesktopAnchor.Anchor(Handle);
            ApplyRoundedRegion();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ApplyRoundedRegion();
        }

        private void ApplyRoundedRegion()
        {
            const int radius = 12;
            using var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, Width, Height);
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == DesktopAnchor.WM_WINDOWPOSCHANGING)
            {
                // OS가 z-order를 바꾸려는 메시지(NOZORDER 안 켜진 것)에 한해
                // hwndInsertAfter를 HWND_BOTTOM으로 리디렉트. "항상 맨 아래" 유지.
                const uint SWP_NOZORDER = 0x0004;
                var pos = Marshal.PtrToStructure<DesktopAnchor.WINDOWPOS>(m.LParam);
                if ((pos.flags & SWP_NOZORDER) == 0)
                {
                    pos.hwndInsertAfter = DesktopAnchor.HWND_BOTTOM;
                    Marshal.StructureToPtr(pos, m.LParam, fDeleteOld: false);
                }
            }

            base.WndProc(ref m);
        }
    }
}
