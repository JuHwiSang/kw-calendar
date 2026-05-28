using KW_Calendar.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalendarEvent = KW_Calendar.Models.Event;

namespace KW_Calendar
{
    public partial class CalendarView : Form
    {
        private readonly ISyncService? syncService;
        private readonly IEventService? eventService;

        private Button refreshButton = null!;
        private Label statusLabel = null!;
        private ListBox eventListBox = null!;

        private bool isRefreshing = false;

        public CalendarView()
        {
            InitializeComponent();
            InitializeRefreshUi();
        }

        public CalendarView(ISyncService syncService, IEventService eventService)
        {
            InitializeComponent();

            this.syncService = syncService;
            this.eventService = eventService;

            InitializeRefreshUi();
        }

        private void InitializeRefreshUi()
        {
            refreshButton = new Button
            {
                Name = "refreshButton",
                Text = "새로고침",
                Width = 130,
                Height = 40,
                Font = new Font("맑은 고딕", 10F, FontStyle.Regular),
                Location = new Point(ClientSize.Width - 150, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            refreshButton.Click += RefreshButton_Click;

            statusLabel = new Label
            {
                Name = "statusLabel",
                Text = "일정 새로고침 대기 중",
                AutoSize = true,
                Font = new Font("맑은 고딕", 10F, FontStyle.Regular),
                Location = new Point(20, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            eventListBox = new ListBox
            {
                Name = "eventListBox",
                Font = new Font("맑은 고딕", 10F, FontStyle.Regular),
                Location = new Point(20, 80),
                Width = Math.Max(400, ClientSize.Width - 40),
                Height = Math.Max(250, ClientSize.Height - 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(statusLabel);
            Controls.Add(refreshButton);
            Controls.Add(eventListBox);
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            if (isRefreshing)
            {
                return;
            }

            if (syncService == null || eventService == null)
            {
                MessageBox.Show(
                    "SyncService 또는 EventService가 연결되어 있지 않습니다.\n\nProgram.cs에서 CalendarView 생성자에 서비스를 전달해야 합니다.",
                    "서비스 연결 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                isRefreshing = true;

                refreshButton.Enabled = false;
                refreshButton.Text = "새로고침 중...";
                statusLabel.Text = "Supabase에서 일정을 불러오는 중입니다...";

                int syncedCount = await syncService.SyncEventsAsync();

                DateTime start = DateTime.Today.AddYears(-1);
                DateTime end = DateTime.Today.AddYears(1);

                var refreshedEvents = await eventService.GetEventsByDateRangeAsync(start, end);

                RenderEvents(refreshedEvents);

                statusLabel.Text = $"새로고침 완료: {syncedCount}개 동기화, {refreshedEvents.Count}개 표시";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "새로고침 실패";

                MessageBox.Show(
                    $"일정 새로고침 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "새로고침 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                isRefreshing = false;

                refreshButton.Enabled = true;
                refreshButton.Text = "새로고침";
            }
        }

        private void RenderEvents(IReadOnlyList<CalendarEvent> events)
        {
            eventListBox.BeginUpdate();

            try
            {
                eventListBox.Items.Clear();

                foreach (CalendarEvent item in events.OrderBy(e => e.StartDt))
                {
                    string startText = item.StartDt.ToString("yyyy-MM-dd HH:mm");
                    string line = $"[{startText}] {item.Title}";

                    eventListBox.Items.Add(line);
                }

                if (events.Count == 0)
                {
                    eventListBox.Items.Add("표시할 일정이 없습니다.");
                }
            }
            finally
            {
                eventListBox.EndUpdate();
            }
        }
    }
}
