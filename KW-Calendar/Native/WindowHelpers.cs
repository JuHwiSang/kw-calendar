using System;
using System.Runtime.InteropServices;

namespace KW_Calendar.Native
{
    internal static class WindowHelpers
    {
        // DWMWA_WINDOW_CORNER_PREFERENCE: Win11 이상에서 윈도우 모서리 둥글기 제어
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;       // 표준 둥글기 (~8px)
        private const int DWMWCP_ROUNDSMALL = 3;  // 작은 둥글기 (~4px)

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
    }
}
