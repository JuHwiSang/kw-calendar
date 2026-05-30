using System.IO.Pipes;

namespace KW_Calendar.Native
{
    /// <summary>
    /// 단일 인스턴스 보장 + 두 번째 인스턴스가 첫 번째를 깨우는 신호 채널.
    /// Mutex로 점유 판정, named pipe로 신호 전달.
    /// </summary>
    public sealed class SingleInstance : IDisposable
    {
        private const string DefaultScope = "{B0A2E7C4-1E0F-4D6B-9A5C-3B7C2D8F4E10}";
        private const int ConnectTimeoutMs = 500;

        private readonly string _mutexName;
        private readonly string _pipeName;
        private readonly Mutex _mutex;
        private readonly bool _owned;
        private CancellationTokenSource? _listenerCts;

        /// <summary>다른 인스턴스가 깨움 신호를 보냈을 때 발생. UI 스레드 호출 보장 없음.</summary>
        public event EventHandler? OpenRequested;

        private SingleInstance(string scope, Mutex mutex, bool owned)
        {
            _mutexName = MakeMutexName(scope);
            _pipeName = MakePipeName(scope);
            _mutex = mutex;
            _owned = owned;
        }

        public bool IsFirstInstance => _owned;

        public static SingleInstance Acquire(string scope = DefaultScope)
        {
            var mutex = new Mutex(initiallyOwned: true, name: MakeMutexName(scope), createdNew: out bool createdNew);
            return new SingleInstance(scope, mutex, createdNew);
        }

        /// <summary>첫 번째 인스턴스에서만 호출. 이후 다른 인스턴스의 신호를 수신한다.</summary>
        public void StartListening(SynchronizationContext? uiContext = null)
        {
            if (!_owned) throw new InvalidOperationException("두 번째 인스턴스는 수신할 수 없습니다.");
            if (_listenerCts != null) return;

            _listenerCts = new CancellationTokenSource();
            _ = ListenLoopAsync(uiContext, _listenerCts.Token);
        }

        /// <summary>두 번째 인스턴스에서 호출. 첫 번째가 듣고 있으면 깨운다.</summary>
        public static void NotifyExisting(string scope = DefaultScope)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", MakePipeName(scope), PipeDirection.Out);
                client.Connect(ConnectTimeoutMs);
                client.WriteByte(1);
                client.Flush();
            }
            catch
            {
                // 첫 번째가 아직 listener 안 띄웠거나 죽었음. 깨우기는 best-effort.
            }
        }

        private async Task ListenLoopAsync(SynchronizationContext? uiContext, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        _pipeName, PipeDirection.In, maxNumberOfServerInstances: 1);
                    await server.WaitForConnectionAsync(token);
                    _ = server.ReadByte();
                    RaiseOpenRequested(uiContext);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // 파이프 오류는 무시하고 다음 연결 대기.
                }
            }
        }

        private void RaiseOpenRequested(SynchronizationContext? uiContext)
        {
            var handler = OpenRequested;
            if (handler == null) return;

            if (uiContext != null)
            {
                uiContext.Post(_ => handler(this, EventArgs.Empty), null);
            }
            else
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _listenerCts?.Cancel();
            _listenerCts?.Dispose();
            if (_owned)
            {
                try { _mutex.ReleaseMutex(); } catch { }
            }
            _mutex.Dispose();
        }

        private static string MakeMutexName(string scope) => $@"Global\KW-Calendar.SingleInstance.{scope}";
        private static string MakePipeName(string scope) => $"KW-Calendar.Wakeup.{scope}";
    }
}
