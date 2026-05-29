using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CalendarApp
{
    public class RoundedPanel : Panel
    {
        private int borderRadius = 18;
        private Color borderColor = Color.FromArgb(229, 231, 235);
        private int borderSize = 1;
        private Color fillColor = Color.White;

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

        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public int BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = value;
                Invalidate();
            }
        }

        public Color FillColor
        {
            get => fillColor;
            set
            {
                fillColor = value;
                Invalidate();
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

            using (GraphicsPath path = GetRoundedPath(rect, BorderRadius))
            using (SolidBrush brush = new SolidBrush(FillColor))
            using (Pen pen = new Pen(BorderColor, BorderSize))
            {
                e.Graphics.FillPath(brush, path);

                if (BorderSize > 0)
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (Width <= 0 || Height <= 0)
                return;

            Rectangle rect = new Rectangle(0, 0, Width, Height);

            using (GraphicsPath path = GetRoundedPath(rect, BorderRadius))
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