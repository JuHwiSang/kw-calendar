using KW_Calendar.Models;
using Microsoft.EntityFrameworkCore;

namespace KW_Calendar.Services;

public class LocalDbService : ILocalDbService, IDisposable
{
    private readonly KwCalendarDbContext _db;

    public LocalDbService(string dbPath)
    {
        _db = new KwCalendarDbContext(dbPath);
        _db.Database.EnsureCreated();
        // EnsureCreated는 신규 컬럼을 추가하지 않으므로 기존 DB에 수동으로 추가
        try { _db.Database.ExecuteSqlRaw("ALTER TABLE Events ADD COLUMN IsOneDayBeforeNotified INTEGER NOT NULL DEFAULT 0"); } catch { }
        try { _db.Database.ExecuteSqlRaw("ALTER TABLE Events ADD COLUMN IsSameDayNotified INTEGER NOT NULL DEFAULT 0"); } catch { }
    }

    public async Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _db.Events
            .Where(e => e.StartDt <= end && e.EndDt >= start)
            .OrderBy(e => e.StartDt)
            .Select(r => r.ToEvent())
            .ToListAsync(ct);
    }

    public async Task<Event?> GetEventByIdAsync(int id, CancellationToken ct = default)
    {
        var row = await _db.Events.FindAsync([id], ct);
        return row?.ToEvent();
    }

    public async Task UpsertEventsAsync(IEnumerable<Event> events, CancellationToken ct = default)
    {
        var eventList = events.ToList();
        var existingMap = new Dictionary<int, EventRow>();

        // SQLite IN (...) 파라미터 최대 999개 제한
        foreach (var chunk in eventList.Chunk(999))
        {
            var chunkIds = chunk.Select(e => e.Id).ToList();
            var rows = await _db.Events
                .Where(r => chunkIds.Contains(r.Id))
                .ToListAsync(ct);
            foreach (var row in rows)
                existingMap[row.Id] = row;
        }

        foreach (var ev in eventList)
        {
            if (existingMap.TryGetValue(ev.Id, out var existing))
            {
                existing.Title = ev.Title;
                existing.Body = ev.Body;
                existing.StartDt = ev.StartDt;
                existing.EndDt = ev.EndDt;
                existing.IsAllDay = ev.IsAllDay;
                existing.NoticeDt = ev.NoticeDt;
                existing.ExternalLink = ev.ExternalLink;
                existing.CategoryId = ev.CategoryId;
                // 클라이언트 전용 필드 보존
                // IsFavorited, IsOneDayBeforeNotified, IsSameDayNotified는 변경하지 않음
            }
            else
            {
                _db.Events.Add(EventRow.FromEvent(ev));
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Event>> GetFavoritedEventsAsync(CancellationToken ct = default)
    {
        // JOIN 대신 두 번의 쿼리로 가져오는 게 간단해서 이렇게 구현
        var favCategoryIds = await _db.Categories
            .Where(c => c.IsFavorited)
            .Select(c => c.Id)
            .ToListAsync(ct);

        return await _db.Events
            .Where(e => e.IsFavorited || favCategoryIds.Contains(e.CategoryId))
            .OrderBy(e => e.StartDt)
            .Select(r => r.ToEvent())
            .ToListAsync(ct);
    }

    public async Task<bool> ToggleEventFavoriteAsync(int eventId, CancellationToken ct = default)
    {
        var row = await _db.Events.FindAsync([eventId], ct)
            ?? throw new InvalidOperationException($"Event {eventId} not found.");
        row.IsFavorited = !row.IsFavorited;
        await _db.SaveChangesAsync(ct);
        return row.IsFavorited;
    }

    public async Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.Categories
            .Select(r => r.ToCategory())
            .ToListAsync(ct);
    }

    public async Task UpsertCategoriesAsync(IEnumerable<Category> categories, CancellationToken ct = default)
    {
        foreach (var cat in categories)
        {
            // 카테고리는 Event와 달리 대량 주입 상황이 많지 않을 것 같아서 단일 조회 후 업데이트/삽입하는 방식으로 구현.
            var existing = await _db.Categories.FindAsync([cat.Id], ct);
            if (existing is null)
            {
                _db.Categories.Add(CategoryRow.FromCategory(cat));
            }
            else
            {
                existing.Name = cat.Name;
                // IsFavorited 보존
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ToggleCategoryFavoriteAsync(int categoryId, CancellationToken ct = default)
    {
        var row = await _db.Categories.FindAsync([categoryId], ct)
            ?? throw new InvalidOperationException($"Category {categoryId} not found.");
        row.IsFavorited = !row.IsFavorited;
        await _db.SaveChangesAsync(ct);
        return row.IsFavorited;
    }

    public async Task MarkEventNotificationsAsync(int eventId, bool? oneDayBefore, bool? sameDay, CancellationToken ct = default)
    {
        var row = await _db.Events.FindAsync([eventId], ct)
            ?? throw new InvalidOperationException($"Event {eventId} not found.");
        if (oneDayBefore.HasValue) row.IsOneDayBeforeNotified = oneDayBefore.Value;
        if (sameDay.HasValue) row.IsSameDayNotified = sameDay.Value;
        await _db.SaveChangesAsync(ct);
    }

    public void Dispose() => _db.Dispose();
}

internal class KwCalendarDbContext : DbContext
{
    private readonly string _dbPath;

    public KwCalendarDbContext(string dbPath) => _dbPath = dbPath;

    public DbSet<EventRow> Events => Set<EventRow>();
    public DbSet<CategoryRow> Categories => Set<CategoryRow>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_dbPath};Pooling=False");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<EventRow>().HasKey(e => e.Id);
        model.Entity<CategoryRow>().HasKey(c => c.Id);
    }
}

internal class EventRow
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public DateTime StartDt { get; set; }
    public DateTime EndDt { get; set; }
    public bool IsAllDay { get; set; }
    public DateTime? NoticeDt { get; set; }
    public string? ExternalLink { get; set; }
    public int CategoryId { get; set; }
    public bool IsFavorited { get; set; }
    public bool IsOneDayBeforeNotified { get; set; }
    public bool IsSameDayNotified { get; set; }

    public Event ToEvent() => new()
    {
        Id = Id,
        Title = Title,
        Body = Body,
        StartDt = StartDt,
        EndDt = EndDt,
        IsAllDay = IsAllDay,
        NoticeDt = NoticeDt,
        ExternalLink = ExternalLink,
        CategoryId = CategoryId,
        IsFavorited = IsFavorited,
        IsOneDayBeforeNotified = IsOneDayBeforeNotified,
        IsSameDayNotified = IsSameDayNotified,
    };

    public static EventRow FromEvent(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Body = e.Body,
        StartDt = e.StartDt,
        EndDt = e.EndDt,
        IsAllDay = e.IsAllDay,
        NoticeDt = e.NoticeDt,
        ExternalLink = e.ExternalLink,
        CategoryId = e.CategoryId,
        IsFavorited = e.IsFavorited,
        IsOneDayBeforeNotified = e.IsOneDayBeforeNotified,
        IsSameDayNotified = e.IsSameDayNotified,
    };
}

internal class CategoryRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsFavorited { get; set; }

    public Category ToCategory() => new()
    {
        Id = Id,
        Name = Name,
        IsFavorited = IsFavorited,
    };

    public static CategoryRow FromCategory(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        IsFavorited = c.IsFavorited,
    };
}
