using System.ComponentModel;
using KW_Calendar.Models;
using KW_Calendar.Native;

namespace KW_Calendar.Views;

public partial class AddEventView : Form, IAddEventView
{
    private readonly CustomTitleBar _titleBar;

    public AddEventView()
    {
        InitializeComponent();

        _titleBar = new CustomTitleBar
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = System.Drawing.Color.White,
            TitleText = "일정 추가",
            ShowMaximize = false,
            ShowMinimize = false,
        };
        Controls.Add(_titleBar);
        _titleBar.BringToFront();

        chkAllDay.CheckedChanged += (s, e) =>
        {
            dtpEnd.Enabled = !chkAllDay.Checked;
        };

        btnConfirm.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("제목을 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!chkAllDay.Checked && dtpEnd.Value <= dtpStart.Value)
            {
                MessageBox.Show("종료 시간은 시작 시간 이후여야 합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Confirmed?.Invoke(this, EventArgs.Empty);
            // Close()는 Presenter가 DB 쓰기 완료 후 CloseView()로 호출한다.
        };

        btnCancel.Click += (s, e) => Close();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowHelpers.ApplyRoundedCorners(Handle);
    }

    // --- IAddEventView ---

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EventTitle => txtTitle.Text.Trim();

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime StartDt => chkAllDay.Checked
        ? dtpStart.Value.Date
        : dtpStart.Value;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime EndDt => chkAllDay.Checked
        ? dtpStart.Value.Date.AddDays(1).AddTicks(-1)
        : dtpEnd.Value;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsAllDay => chkAllDay.Checked;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Body => string.IsNullOrWhiteSpace(txtBody.Text) ? null : txtBody.Text.Trim();

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CategoryId => cmbCategory.SelectedItem is Category cat ? cat.Id : 0;

    public void SetCategories(IReadOnlyList<Category> categories)
    {
        cmbCategory.Items.Clear();
        cmbCategory.Items.Add("(없음)");
        foreach (var cat in categories)
            cmbCategory.Items.Add(cat);
        cmbCategory.SelectedIndex = 0;
        cmbCategory.DisplayMember = "Name";
    }

    public void SetInitialDate(DateOnly date)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        dtpStart.Value = dt;
        dtpEnd.Value = dt.AddHours(1);
    }

    public void SetEvent(Event e)
    {
        txtTitle.Text = e.Title;
        dtpStart.Value = e.StartDt == default ? DateTime.Now : e.StartDt;
        dtpEnd.Value = e.EndDt ?? e.StartDt.AddHours(1);
        chkAllDay.Checked = e.IsAllDay;
        txtBody.Text = e.Body ?? string.Empty;

        // 카테고리 선택: SetCategories 이후에 호출되어야 함
        for (int i = 0; i < cmbCategory.Items.Count; i++)
        {
            if (cmbCategory.Items[i] is Category cat && cat.Id == e.CategoryId)
            {
                cmbCategory.SelectedIndex = i;
                return;
            }
        }
        cmbCategory.SelectedIndex = 0; // (없음)
    }

    public void SetTitle(string title) => _titleBar.TitleText = title;

    public void SetConfirmButtonText(string text) => btnConfirm.Text = text;

    public void CloseView() => Close();

    public event EventHandler? Confirmed;
}
