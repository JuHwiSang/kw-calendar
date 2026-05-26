using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using KW_Calendar.Models;
using KW_Calendar.Native;
using Microsoft.Web.WebView2.Core;

namespace KW_Calendar.Views
{
    /// <summary>
    /// 바탕화면 위에 떠 있는 위젯. WebView2로 HTML/CSS UI를 호스팅한다.
    ///
    /// === "바탕화면 위, 다른 모든 창 아래" 트릭 ===
    /// 1) SetParent로 Progman/WorkerW에 직접 부착하는 방식은 Win11 raised desktop
    ///    환경에서 layered SHELLDLL_DefView가 마우스 hit-test를 가로채 클릭이
    ///    먹지 않게 된다(아이콘 위로 마우스가 가는 것처럼 처리됨).
    /// 2) 그래서 우리는 일반 top-level 윈도우로 두되, WM_WINDOWPOSCHANGING에서
    ///    Z순서를 매번 HWND_BOTTOM으로 강제하여 시각상 "바탕화면 위, 다른 창 아래"를
    ///    유지한다. (Rainmeter, VueMinder 등이 사용하는 방식)
    /// 3) WS_EX_TOOLWINDOW로 작업표시줄/Alt+Tab에서 제외, WS_EX_NOACTIVATE로
    ///    클릭해도 포커스를 가져가지 않게 한다.
    /// 4) Win+D ("바탕화면 보기") 대응:
    ///    Show Desktop은 WS_MINIMIZEBOX 스타일이 있는 창만 최소화한다.
    ///    → CreateParams에서 WS_MINIMIZEBOX 비트를 끄면 Win+D가 위젯을 건너뛴다.
    ///    참고: https://devblogs.microsoft.com/oldnewthing/20241021-00/?p=110393
    /// </summary>
    public partial class CalendarWidgetView : Form, ICalendarView
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };

        private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> _eventsByDay
            = new Dictionary<DateOnly, IReadOnlyList<Event>>();
        private IReadOnlyList<Event> _events = Array.Empty<Event>();
        private IReadOnlyList<Category> _categories = Array.Empty<Category>();

        private bool _webReady;
        private readonly Queue<Action> _pendingJsCalls = new();

        public CalendarWidgetView()
        {
            InitializeComponent();
            _ = InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            // UserData 폴더를 명시 지정 (안 하면 exe 옆에 멋대로 생성됨)
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KW-Calendar", "WebView2");
            Directory.CreateDirectory(userDataFolder);
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await _webView.EnsureCoreWebView2Async(env);

            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            // 투명 배경 (위젯 느낌)
            _webView.DefaultBackgroundColor = Color.Transparent;

            var htmlPath = Path.Combine(AppContext.BaseDirectory, "Views", "widget.html");
            _webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var type = doc.RootElement.GetProperty("type").GetString();

            switch (type)
            {
                case "ready":
                    _webReady = true;
                    FlushPendingJsCalls();
                    PushAllToJs();
                    break;
                case "prevMonth":
                    PreviousMonthRequested?.Invoke(this, EventArgs.Empty);
                    break;
                case "nextMonth":
                    NextMonthRequested?.Invoke(this, EventArgs.Empty);
                    break;
                case "eventSelected":
                    EventSelected?.Invoke(this, doc.RootElement.GetProperty("id").GetInt32());
                    break;
                case "eventFavoriteToggle":
                    EventFavoriteToggleRequested?.Invoke(this, doc.RootElement.GetProperty("id").GetInt32());
                    break;
                case "categoryFavoriteToggle":
                    CategoryFavoriteToggleRequested?.Invoke(this, doc.RootElement.GetProperty("id").GetInt32());
                    break;
                case "close":
                    // TODO: NotifyIcon 도입 후 Hide()로 변경 (별도 이슈).
                    Close();
                    break;
                case "beginDrag":
                    BeginSystemDrag();
                    break;
            }
        }

        private void BeginSystemDrag()
        {
            // WebView2가 마우스를 캡처한 상태이므로 먼저 캡처 해제.
            ReleaseCapture();
            // 시스템 드래그 모드 진입. Windows가 마우스 이동/릴리즈를 알아서 처리한다.
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int HTCAPTION = 2;
            SendMessage(Handle, WM_NCLBUTTONDOWN, new IntPtr(HTCAPTION), IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private void FlushPendingJsCalls()
        {
            while (_pendingJsCalls.Count > 0)
            {
                _pendingJsCalls.Dequeue()();
            }
        }

        private void PushAllToJs()
        {
            PushMonth();
            PushEvents();
            PushFavEvents();
            PushCategories();
        }

        private void RunJs(Action call)
        {
            if (_webReady)
            {
                try { call(); }
                catch { /* WebView 종료 중일 수 있음 */ }
            }
            else
            {
                _pendingJsCalls.Enqueue(call);
            }
        }

        private void PushMonth()
            => RunJs(() => _webView.CoreWebView2.ExecuteScriptAsync(
                $"window.widget.setMonth({JsonSerializer.Serialize($"{_displayedMonth:yyyy년 M월}", JsonOpts)})"));

        private void PushEvents()
        {
            var start = DateOnly.FromDateTime(_displayedMonth);
            var end = DateOnly.FromDateTime(_displayedMonth.AddMonths(1).AddDays(-1));
            var flat = _eventsByDay
                .Where(kv => kv.Key >= start && kv.Key <= end)
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .Select(ToEventDto)
                .ToList();
            var json = JsonSerializer.Serialize(flat, JsonOpts);
            RunJs(() => _webView.CoreWebView2.ExecuteScriptAsync($"window.widget.setEvents({json})"));
        }

        private void PushFavEvents()
        {
            var dto = _events.Select(ToEventDto).ToList();
            var json = JsonSerializer.Serialize(dto, JsonOpts);
            RunJs(() => _webView.CoreWebView2.ExecuteScriptAsync($"window.widget.setFavEvents({json})"));
        }

        private void PushCategories()
        {
            var dto = _categories.Select(c => new { id = c.Id, name = c.Name, isFavorited = c.IsFavorited }).ToList();
            var json = JsonSerializer.Serialize(dto, JsonOpts);
            RunJs(() => _webView.CoreWebView2.ExecuteScriptAsync($"window.widget.setCategories({json})"));
        }

        private static object ToEventDto(Event ev) => new
        {
            id = ev.Id,
            title = ev.Title,
            startDt = ev.StartDt.ToString("o"),
            isFavorited = ev.IsFavorited,
        };

        // --- 바탕화면 앵커링 윈도우 스타일 (클래스 docstring 3, 4번 참조) ---
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_NOACTIVATE = 0x08000000;

                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                cp.Style &= ~WS_MINIMIZEBOX;  // ← Win+D 통과의 핵심
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DesktopAnchor.Anchor(Handle);
            ApplyRoundedRegion();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ApplyRoundedRegion();
        }

        private void ApplyRoundedRegion()
        {
            const int radius = 12;
            using var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, Width, Height);
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }

        protected override void WndProc(ref Message m)
        {
            // 다른 창이 활성화될 때 우리 창이 Z순서 위로 올라가려는 시도를 막는다.
            // (클래스 docstring 2번 참조)
            if (m.Msg == DesktopAnchor.WM_WINDOWPOSCHANGING)
            {
                var pos = Marshal.PtrToStructure<DesktopAnchor.WINDOWPOS>(m.LParam);
                pos.hwndInsertAfter = DesktopAnchor.HWND_BOTTOM;
                Marshal.StructureToPtr(pos, m.LParam, fDeleteOld: false);
            }

            base.WndProc(ref m);
        }

        // --- ICalendarView ---

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DateTime DisplayedMonth
        {
            get => _displayedMonth;
            set
            {
                _displayedMonth = value;
                PushMonth();
                PushEvents();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>> EventsByDay
        {
            get => _eventsByDay;
            set
            {
                _eventsByDay = value ?? new Dictionary<DateOnly, IReadOnlyList<Event>>();
                PushEvents();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new IReadOnlyList<Event> Events
        {
            get => _events;
            set
            {
                _events = value ?? Array.Empty<Event>();
                PushFavEvents();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value ?? Array.Empty<Category>();
                PushCategories();
            }
        }

        public event EventHandler? PreviousMonthRequested;
        public event EventHandler? NextMonthRequested;
        public event EventHandler<int>? EventSelected;
        public event EventHandler<int>? EventFavoriteToggleRequested;
        public event EventHandler<int>? CategoryFavoriteToggleRequested;
    }
}
