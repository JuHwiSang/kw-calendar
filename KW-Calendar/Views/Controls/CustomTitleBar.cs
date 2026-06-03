using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KW_Calendar.Views
{
    /// <summary>
    /// FormBorderStyle.None 폼에 붙여 쓰는 커스텀 타이틀 바.
    /// 제목 텍스트 + 최소화/최대화(토글)/닫기 버튼 + 드래그 이동 지원.
    /// </summary>
    public class CustomTitleBar : Panel
    {
        private static readonly Font TitleFont = new("맑은 고딕", 10F, FontStyle.Bold);
        private static readonly Font ButtonFont = new("Segoe UI Symbol", 11F, FontStyle.Regular);
        private static readonly Color TitleFore = Color.FromArgb(31, 41, 55);
        private static readonly Color ButtonFore = Color.FromArgb(107, 114, 128);
        private static readonly Color ButtonHover = Color.FromArgb(229, 231, 235);
        private static readonly Color CloseHover = Color.FromArgb(239, 68, 68);
        private static readonly Color CloseHoverFore = Color.White;

        private readonly Label _title;
        private readonly Label _btnMin;
        private readonly Label _btnMax;
        private readonly Label _btnClose;

        public CustomTitleBar()
        {
            Height = 36;
            BackColor = Color.White;
            Dock = DockStyle.Top;

            // 버튼은 우측부터 Dock.Right로 깔린다. Add 순서대로 우→좌.
            _btnClose = MakeButton("✕", isClose: true);
            _btnMax = MakeButton("□", isClose: false);
            _btnMin = MakeButton("─", isClose: false);

            _btnClose.Click += (s, e) => OwnerForm?.Close();
            _btnMax.Click += (s, e) => ToggleMaximize();
            _btnMin.Click += (s, e) =>
            {
                var f = OwnerForm;
                if (f != null) f.WindowState = FormWindowState.Minimized;
            };

            _title = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = TitleFont,
                ForeColor = TitleFore,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 0, 0, 0),
                Dock = DockStyle.Fill
            };

            // Dock.Right는 나중에 add된 컨트롤이 더 우측에 깔린다.
            // 최종 시각 순서(왼→오): ─ □ ×
            Controls.Add(_title);
            Controls.Add(_btnMin);
            Controls.Add(_btnMax);
            Controls.Add(_btnClose);

            EnableDrag();
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TitleText
        {
            get => _title.Text;
            set => _title.Text = value ?? string.Empty;
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool ShowMinimize
        {
            get => _btnMin.Visible;
            set => _btnMin.Visible = value;
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool ShowMaximize
        {
            get => _btnMax.Visible;
            set => _btnMax.Visible = value;
        }

        private Form? OwnerForm => FindForm();

        private Label MakeButton(string glyph, bool isClose)
        {
            var btn = new Label
            {
                Text = glyph,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = ButtonFont,
                ForeColor = ButtonFore,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Width = 44,
                Dock = DockStyle.Right
            };

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = isClose ? CloseHover : ButtonHover;
                if (isClose) btn.ForeColor = CloseHoverFore;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = ButtonFore;
            };

            return btn;
        }

        private void ToggleMaximize()
        {
            var f = OwnerForm;
            if (f == null) return;
            f.WindowState = f.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        // --- 드래그 이동 (Win32 메시지로 폼 드래그) ---

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void EnableDrag()
        {
            MouseDown += OnDragStart;
            _title.MouseDown += OnDragStart;
            DoubleClick += (s, e) => { if (ShowMaximize) ToggleMaximize(); };
            _title.DoubleClick += (s, e) => { if (ShowMaximize) ToggleMaximize(); };
        }

        private void OnDragStart(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var f = OwnerForm;
            if (f == null) return;
            ReleaseCapture();
            SendMessage(f.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
    }
}
