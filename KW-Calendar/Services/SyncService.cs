using KW_Calendar.Models;
using Microsoft.Extensions.Configuration;
using Postgrest.Attributes;
using Postgrest.Models;
using Supabase;

namespace KW_Calendar.Services;

public class SyncService : ISyncService
{
    private readonly Client _supabase;
    private readonly ILocalDbService _localDb;
    private volatile bool _isSyncing;

    public bool IsSyncing => _isSyncing;

    public SyncService(IConfiguration config, ILocalDbService localDb)
    {
        var url     = config["Supabase:Url"]!;
        var anonKey = config["Supabase:AnonKey"]!;
        _supabase   = new Client(url, anonKey);
        _localDb    = localDb;
    }

    public async Task<int> SyncEventsAsync(CancellationToken ct = default)
    {
        if (_isSyncing) return 0;
        _isSyncing = true;
        try
        {
            // Categories 먼저 (events가 category_id 외래키 참조)
            var catResponse = await _supabase.From<SupabaseCategory>().Get(ct);
            var categories  = catResponse.Models.Select(ToCategory).ToList();
            await _localDb.UpsertCategoriesAsync(categories, ct);

            var evResponse = await _supabase.From<SupabaseEvent>().Get(ct);
            var events     = evResponse.Models.Select(ToEvent).ToList();
            await _localDb.UpsertEventsAsync(events, ct);

            return events.Count;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private static Event ToEvent(SupabaseEvent s) => new()
    {
        Id           = s.Id,
        Title        = s.Title,
        Body         = s.Body,
        StartDt      = s.StartDt,
        EndDt        = s.EndDt,
        IsAllDay     = s.IsAllDay,
        NoticeDt     = s.NoticeDt,
        ExternalLink = s.ExternalLink,
        CategoryId   = s.CategoryId,
        // IsFavorited는 로컬 전용 — UpsertEventsAsync에서 기존 값 보존
    };

    private static Category ToCategory(SupabaseCategory s) => new()
    {
        Id   = s.Id,
        Name = s.Name,
        // IsFavorited는 로컬 전용
    };

    [Table("events")]
    private class SupabaseEvent : BaseModel
    {
        [PrimaryKey("id", false)]   public int       Id           { get; set; }
        [Column("title")]           public string    Title        { get; set; } = "";
        [Column("body")]            public string?   Body         { get; set; }
        [Column("start_dt")]        public DateTime  StartDt      { get; set; }
        [Column("end_dt")]          public DateTime  EndDt        { get; set; }
        [Column("is_all_day")]      public bool      IsAllDay     { get; set; }
        [Column("notice_dt")]       public DateTime? NoticeDt     { get; set; }
        [Column("external_link")]   public string?   ExternalLink { get; set; }
        [Column("category_id")]     public int       CategoryId   { get; set; }
    }

    [Table("categories")]
    private class SupabaseCategory : BaseModel
    {
        [PrimaryKey("id", false)]   public int    Id   { get; set; }
        [Column("name")]            public string Name { get; set; } = "";
    }
}
