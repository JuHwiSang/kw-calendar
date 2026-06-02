namespace KW_Calendar.Views;

partial class EventDetailView
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblTime;
    private System.Windows.Forms.Label lblBody;
    private System.Windows.Forms.Button btnFavorite;
    private System.Windows.Forms.Button btnOpenExternalLink;

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
        lblTitle = new System.Windows.Forms.Label();
        lblTime = new System.Windows.Forms.Label();
        lblBody = new System.Windows.Forms.Label();
        btnFavorite = new System.Windows.Forms.Button();
        btnOpenExternalLink = new System.Windows.Forms.Button();

        SuspendLayout();

        // 
        // lblTitle
        // 
        lblTitle.AutoSize = false;
        lblTitle.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.FromArgb(31, 41, 55);
        lblTitle.Location = new System.Drawing.Point(28, 24);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new System.Drawing.Size(340, 36);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "일정 제목";
        lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        // 
        // btnFavorite
        // 
        btnFavorite.FlatAppearance.BorderSize = 0;
        btnFavorite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnFavorite.Font = new System.Drawing.Font("Segoe UI Symbol", 18F, System.Drawing.FontStyle.Regular);
        btnFavorite.ForeColor = System.Drawing.Color.FromArgb(250, 204, 21);
        btnFavorite.Location = new System.Drawing.Point(374, 21);
        btnFavorite.Name = "btnFavorite";
        btnFavorite.Size = new System.Drawing.Size(44, 44);
        btnFavorite.TabIndex = 1;
        btnFavorite.Text = "☆";
        btnFavorite.UseVisualStyleBackColor = true;

        // 
        // lblTime
        // 
        lblTime.AutoSize = false;
        lblTime.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
        lblTime.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
        lblTime.Location = new System.Drawing.Point(30, 76);
        lblTime.Name = "lblTime";
        lblTime.Size = new System.Drawing.Size(388, 28);
        lblTime.TabIndex = 2;
        lblTime.Text = "2026.01.01 00:00 - 00:00";
        lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        // 
        // lblBody
        // 
        lblBody.AutoSize = false;
        lblBody.BackColor = System.Drawing.Color.FromArgb(249, 250, 251);
        lblBody.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        lblBody.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular);
        lblBody.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
        lblBody.Location = new System.Drawing.Point(30, 122);
        lblBody.Name = "lblBody";
        lblBody.Padding = new System.Windows.Forms.Padding(12);
        lblBody.Size = new System.Drawing.Size(388, 210);
        lblBody.TabIndex = 3;
        lblBody.Text = "상세 내용";
        lblBody.TextAlign = System.Drawing.ContentAlignment.TopLeft;

        // 
        // btnOpenExternalLink
        // 
        btnOpenExternalLink.BackColor = System.Drawing.Color.FromArgb(136, 19, 55);
        btnOpenExternalLink.FlatAppearance.BorderSize = 0;
        btnOpenExternalLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnOpenExternalLink.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
        btnOpenExternalLink.ForeColor = System.Drawing.Color.White;
        btnOpenExternalLink.Location = new System.Drawing.Point(30, 352);
        btnOpenExternalLink.Name = "btnOpenExternalLink";
        btnOpenExternalLink.Size = new System.Drawing.Size(388, 42);
        btnOpenExternalLink.TabIndex = 4;
        btnOpenExternalLink.Text = "외부 링크 열기";
        btnOpenExternalLink.UseVisualStyleBackColor = false;

        // 
        // EventDetailView
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(450, 420);
        Controls.Add(btnOpenExternalLink);
        Controls.Add(lblBody);
        Controls.Add(lblTime);
        Controls.Add(btnFavorite);
        Controls.Add(lblTitle);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "EventDetailView";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "일정 상세";

        ResumeLayout(false);
    }
}
