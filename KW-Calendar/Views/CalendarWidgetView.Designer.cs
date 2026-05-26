using Microsoft.Web.WebView2.WinForms;

namespace KW_Calendar.Views
{
    partial class CalendarWidgetView
    {
        private System.ComponentModel.IContainer components = null;

        private WebView2 _webView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _webView = new WebView2();

            SuspendLayout();

            _webView.Dock = DockStyle.Fill;
            _webView.DefaultBackgroundColor = Color.Transparent;

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(320, 480);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "KW-Calendar Widget";

            Controls.Add(_webView);

            ResumeLayout(false);
        }
    }
}
