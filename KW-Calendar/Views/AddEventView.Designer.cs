namespace KW_Calendar.Views;

partial class AddEventView
{
    private System.ComponentModel.IContainer components = null;

    private Panel contentArea;
    private Label lblTitleCaption;
    private TextBox txtTitle;
    private Label lblStartCaption;
    private DateTimePicker dtpStart;
    private Label lblEndCaption;
    private DateTimePicker dtpEnd;
    private CheckBox chkAllDay;
    private Label lblBodyCaption;
    private TextBox txtBody;
    private Label lblCategoryCaption;
    private ComboBox cmbCategory;
    private Panel footerArea;
    private Button btnConfirm;
    private Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        contentArea = new Panel();
        lblTitleCaption = new Label();
        txtTitle = new TextBox();
        lblStartCaption = new Label();
        dtpStart = new DateTimePicker();
        lblEndCaption = new Label();
        dtpEnd = new DateTimePicker();
        chkAllDay = new CheckBox();
        lblBodyCaption = new Label();
        txtBody = new TextBox();
        lblCategoryCaption = new Label();
        cmbCategory = new ComboBox();
        footerArea = new Panel();
        btnConfirm = new Button();
        btnCancel = new Button();

        SuspendLayout();
        contentArea.SuspendLayout();
        footerArea.SuspendLayout();

        // AddEventView
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(360, 520);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = System.Drawing.Color.White;
        Name = "AddEventView";

        // contentArea
        contentArea.Dock = DockStyle.Fill;
        contentArea.Padding = new Padding(24, 60, 24, 0);
        contentArea.Controls.AddRange(new Control[]
        {
            lblTitleCaption, txtTitle,
            lblStartCaption, dtpStart,
            lblEndCaption, dtpEnd,
            chkAllDay,
            lblBodyCaption, txtBody,
            lblCategoryCaption, cmbCategory,
        });

        // lblTitleCaption
        lblTitleCaption.Text = "제목 *";
        lblTitleCaption.Location = new System.Drawing.Point(24, 60);
        lblTitleCaption.Size = new System.Drawing.Size(60, 20);

        // txtTitle
        txtTitle.Location = new System.Drawing.Point(24, 82);
        txtTitle.Size = new System.Drawing.Size(312, 23);

        // lblStartCaption
        lblStartCaption.Text = "시작";
        lblStartCaption.Location = new System.Drawing.Point(24, 118);
        lblStartCaption.Size = new System.Drawing.Size(60, 20);

        // dtpStart
        dtpStart.Location = new System.Drawing.Point(24, 140);
        dtpStart.Size = new System.Drawing.Size(312, 23);
        dtpStart.Format = DateTimePickerFormat.Custom;
        dtpStart.CustomFormat = "yyyy-MM-dd HH:mm";

        // lblEndCaption
        lblEndCaption.Text = "종료";
        lblEndCaption.Location = new System.Drawing.Point(24, 176);
        lblEndCaption.Size = new System.Drawing.Size(60, 20);

        // dtpEnd
        dtpEnd.Location = new System.Drawing.Point(24, 198);
        dtpEnd.Size = new System.Drawing.Size(312, 23);
        dtpEnd.Format = DateTimePickerFormat.Custom;
        dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";

        // chkAllDay
        chkAllDay.Text = "종일";
        chkAllDay.Location = new System.Drawing.Point(24, 232);
        chkAllDay.Size = new System.Drawing.Size(80, 20);

        // lblBodyCaption
        lblBodyCaption.Text = "내용 (선택)";
        lblBodyCaption.Location = new System.Drawing.Point(24, 262);
        lblBodyCaption.Size = new System.Drawing.Size(80, 20);

        // txtBody
        txtBody.Location = new System.Drawing.Point(24, 284);
        txtBody.Size = new System.Drawing.Size(312, 60);
        txtBody.Multiline = true;
        txtBody.ScrollBars = ScrollBars.Vertical;

        // lblCategoryCaption
        lblCategoryCaption.Text = "카테고리 (선택)";
        lblCategoryCaption.Location = new System.Drawing.Point(24, 356);
        lblCategoryCaption.Size = new System.Drawing.Size(100, 20);

        // cmbCategory
        cmbCategory.Location = new System.Drawing.Point(24, 378);
        cmbCategory.Size = new System.Drawing.Size(312, 23);
        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;

        // footerArea
        footerArea.Dock = DockStyle.Bottom;
        footerArea.Height = 56;
        footerArea.Padding = new Padding(24, 8, 24, 8);
        footerArea.Controls.AddRange(new Control[] { btnCancel, btnConfirm });

        // btnConfirm
        btnConfirm.Text = "추가";
        btnConfirm.Size = new System.Drawing.Size(80, 32);
        btnConfirm.Location = new System.Drawing.Point(228, 8);
        btnConfirm.BackColor = System.Drawing.Color.FromArgb(136, 19, 55);
        btnConfirm.ForeColor = System.Drawing.Color.White;
        btnConfirm.FlatStyle = FlatStyle.Flat;
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Cursor = Cursors.Hand;

        // btnCancel
        btnCancel.Text = "취소";
        btnCancel.Size = new System.Drawing.Size(80, 32);
        btnCancel.Location = new System.Drawing.Point(136, 8);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Cursor = Cursors.Hand;

        footerArea.ResumeLayout(false);
        contentArea.ResumeLayout(false);
        Controls.Add(contentArea);
        Controls.Add(footerArea);
        ResumeLayout(false);
    }
}
