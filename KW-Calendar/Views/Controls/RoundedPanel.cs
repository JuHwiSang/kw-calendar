using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KW_Calendar.Views
{
    public class RoundedPanel : Panel
    {
        private int borderRadius = 18;
        private Color borderColor = Color.FromArgb(229, 231, 235);
        private int borderSize = 1;
        private Color fillColor = Color.White;
        private bool enableRegion = false;

        private SolidBrush? cachedBrush;
        private Pen? cachedPen;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                borderRadius = value;
                Invalidate();
                UpdateRegion();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                cachedPen?.Dispose();
                cachedPen = null;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = value;
                cachedPen?.Dispose();
                cachedPen = null;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FillColor
        {
            get => fillColor;
            set
            {
                fillColor = value;
                cachedBrush?.Dispose();
                cachedBrush = null;
                Invalidate();
            }
        }

        /// <summary>
        /// true면 GDI Region을 적용해 컨트롤 hit-test와 자식 클리핑이 둥근 모양을 따른다.
        /// 비용이 크므로 기본은 false. OnPaint 자체는 EnableRegion과 무관하게 둥글게 그린다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableRegion
        {
            get => enableRegion;
            set
            {
                enableRegion = value;
                if (!enableRegion)
                    Region = null;
                else
                    UpdateRegion();
            }
        }

        public RoundedPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width <= 0 || Height <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            cachedBrush ??= new SolidBrush(fillColor);
            using (GraphicsPath path = GetRoundedPath(rect, borderRadius))
            {
                e.Graphics.FillPath(cachedBrush, path);

                if (borderSize > 0)
                {
                    cachedPen ??= new Pen(borderColor, borderSize);
                    e.Graphics.DrawPath(cachedPen, path);
                }
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            if (enableRegion)
                UpdateRegion();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cachedBrush?.Dispose();
                cachedPen?.Dispose();
                cachedBrush = null;
                cachedPen = null;
            }
            base.Dispose(disposing);
        }

        private void UpdateRegion()
        {
            if (!enableRegion) return;
            if (Width <= 0 || Height <= 0) return;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            using (GraphicsPath path = GetRoundedPath(rect, borderRadius))
            {
                Region = new Region(path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int safeRadius = Math.Max(0, radius);
            int diameter = safeRadius * 2;

            if (diameter > rect.Width)
                diameter = rect.Width;

            if (diameter > rect.Height)
                diameter = rect.Height;

            if (diameter <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
