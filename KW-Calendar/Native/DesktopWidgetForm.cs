using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    /// <summary>
    /// 바탕화면 위에 떠 있는 위젯의 공용 동작을 담는 베이스 Form.
    ///
    /// === "바탕화면 위, 다른 모든 창 아래" 트릭 ===
    /// 1) SetParent로 Progman/WorkerW에 직접 부착하는 방식은 Win11 raised desktop
    ///    환경에서 layered SHELLDLL_DefView가 마우스 hit-test를 가로채 클릭이
    ///    먹지 않게 된다(아이콘 위로 마우스가 가는 것처럼 처리됨).
    /// 2) 그래서 우리는 일반 top-level 윈도우로 두되, WM_WINDOWPOSCHANGING에서
    ///    Z순서를 매번 HWND_BOTTOM으로 강제하여 시각상 "바탕화면 위, 다른 창 아래"를
    ///    유지한다. (Rainmeter, VueMinder 등이 사용하는 방식)
    /// 3) WS_EX_TOOLWINDOW로 작업표시줄/Alt+Tab에서 제외, WS_EX_NOACTIVATE로
    ///    클릭해도 포커스를 가져가지 않게 한다.
    /// 4) Win+D ("바탕화면 보기") 대응:
    ///    Show Desktop은 WS_MINIMIZEBOX 스타일이 있는 창만 최소화한다.
    ///    → CreateParams에서 WS_MINIMIZEBOX 비트를 끄면 Win+D가 위젯을 건너뛴다.
    ///    참고: https://devblogs.microsoft.com/oldnewthing/20241021-00/?p=110393
    ///
    /// TODO: 프로그램 처음 뜰 때 위젯이 일반 창처럼 위에 뜬다.
    ///       사소한 이슈라 보류. 이후 Z순서 강제 로직 복원할 때 같이 해결.
    /// </summary>
    public class DesktopWidgetForm : Form
    {
        protected void BeginSystemDrag()
        {
            // WebView2가 마우스를 캡처한 상태이므로 먼저 캡처 해제.
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

        // --- 바탕화면 앵커링 윈도우 스타일 (클래스 docstring 3, 4번 참조) ---
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

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
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
            switch (m.Msg)
            {
                case DesktopAnchor.WM_WINDOWPOSCHANGING:
                case 0x0047: // WM_WINDOWPOSCHANGED
                {
                    var pos = Marshal.PtrToStructure<DesktopAnchor.WINDOWPOS>(m.LParam);
                    break;
                }
                case 0x0018: // WM_SHOWWINDOW
                    break;
                case 0x001C: // WM_ACTIVATEAPP
                    break;
                case 0x0006: // WM_ACTIVATE
                    break;
                case 0x0086: // WM_NCACTIVATE
                    break;
                case 0x0005: // WM_SIZE
                    break;
                case 0x0112: // WM_SYSCOMMAND
                    break;
            }

            base.WndProc(ref m);
        }
    }
}
