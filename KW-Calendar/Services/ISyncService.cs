namespace KW_Calendar.Services;

public interface ISyncService
{
    // Supabase REST에서 받아와 로컬 SQLite에 upsert. IsFavorited 등 클라이언트 전용 필드는 보존.
    Task<int> SyncEventsAsync(CancellationToken ct = default);
    bool IsSyncing { get; }
}
