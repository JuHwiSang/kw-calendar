using KW_Calendar.Models;
using Microsoft.Extensions.Configuration;
using Postgrest.Attributes;
using Postgrest.Models;
using Supabase;

namespace KW_Calendar.Services;

public class SupabaseService : ISupabaseService
{
    private readonly Client _client;

    public SupabaseService(IConfiguration config)
    {
        var url     = config["Supabase:Url"]!;
        var anonKey = config["Supabase:AnonKey"]!;
        _client     = new Client(url, anonKey);
    }

    public async Task<IReadOnlyList<Event>> FetchEventsAsync(CancellationToken ct = default)
    {
        var response = await _client.From<SupabaseEvent>().Get(ct);
        return response.Models.Select(ToEvent).ToList();
    }

    public async Task<IReadOnlyList<Category>> FetchCategoriesAsync(CancellationToken ct = default)
    {
        var response = await _client.From<SupabaseCategory>().Get(ct);
        return response.Models.Select(ToCategory).ToList();
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
    };

    private static Category ToCategory(SupabaseCategory s) => new()
    {
        Id   = s.Id,
        Name = s.Name,
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
