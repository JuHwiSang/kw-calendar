using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KW_Calendar.Native
{
    internal static class WindowHelpers
    {
        // DWMWA_WINDOW_CORNER_PREFERENCE: Win11 이상에서 윈도우 모서리 둥글기 제어
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;       // 표준 둥글기 (~8px)
        private const int DWMWCP_ROUNDSMALL = 3;  // 작은 둥글기 (~4px)

        public const int WM_NCHITTEST = 0x0084;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// Win11에서 폼 전체에 시스템 표준 둥근 모서리 적용. Win10 이하는 무시.
        /// </summary>
        public static void ApplyRoundedCorners(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;
            int pref = DWMWCP_ROUND;
            try
            {
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, sizeof(int));
            }
            catch
            {
                // DWM API 미지원 환경에서는 조용히 무시.
            }
        }

        /// <summary>
        /// 폼의 client 좌표 pt가 가장자리 border 영역에 들어가면 해당 HT* 코드를 반환.
        /// 아니면 null.
        /// </summary>
        public static int? GetResizeHitCode(Form form, Point pt, int border)
        {
            if (form.WindowState != FormWindowState.Normal) return null;

            bool left = pt.X < border;
            bool right = pt.X >= form.ClientSize.Width - border;
            bool top = pt.Y < border;
            bool bottom = pt.Y >= form.ClientSize.Height - border;

            return (left, top, right, bottom) switch
            {
                (true, true, _, _) => HTTOPLEFT,
                (_, true, true, _) => HTTOPRIGHT,
                (true, _, _, true) => HTBOTTOMLEFT,
                (_, _, true, true) => HTBOTTOMRIGHT,
                (true, _, _, _) => HTLEFT,
                (_, _, true, _) => HTRIGHT,
                (_, true, _, _) => HTTOP,
                (_, _, _, true) => HTBOTTOM,
                _ => (int?)null,
            };
        }
    }
}
