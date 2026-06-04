using System;
using System.ComponentModel;
using System.Windows.Forms;
using KW_Calendar.Native;

namespace KW_Calendar.Views
{
    /// <summary>
    /// FormBorderStyle.None 폼의 가장자리 리사이즈를 가능하게 해주는 Panel.
    /// 가장자리 ResizeBorder 영역에 마우스가 오면 WM_NCHITTEST에서 HTTRANSPARENT를 돌려줘
    /// 부모 폼이 메시지를 받아 리사이즈를 처리하게 한다.
    /// </summary>
    public class ResizablePanel : Panel
    {
        private const int HTTRANSPARENT = -1;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ResizeBorder { get; set; } = 6;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WindowHelpers.WM_NCHITTEST)
            {
                int x = (short)(m.LParam.ToInt32() & 0xFFFF);
                int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);
                var pt = PointToClient(new System.Drawing.Point(x, y));

                bool nearEdge =
                    pt.X < ResizeBorder ||
                    pt.X >= Width - ResizeBorder ||
                    pt.Y < ResizeBorder ||
                    pt.Y >= Height - ResizeBorder;

                if (nearEdge)
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
