namespace KW_Calendar.Services;

public class SyncService : ISyncService
{
    private readonly ISupabaseService _supabase;
    private readonly ILocalDbService _localDb;
    private volatile bool _isSyncing;

    public bool IsSyncing => _isSyncing;

    public SyncService(ISupabaseService supabase, ILocalDbService localDb)
    {
        _supabase = supabase;
        _localDb  = localDb;
    }

    public async Task<int> SyncEventsAsync(CancellationToken ct = default)
    {
        if (_isSyncing) return 0;
        _isSyncing = true;
        try
        {
            // Categories 먼저 (events가 category_id 외래키 참조)
            var categories = await _supabase.FetchCategoriesAsync(ct);
            await _localDb.UpsertCategoriesAsync(categories, ct);

            var events = await _supabase.FetchEventsAsync(ct);
            await _localDb.UpsertEventsAsync(events, ct);

            return events.Count;
        }
        finally
        {
            _isSyncing = false;
        }
    }
}
