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
        foreach (var ev in events)
        {
            var existing = await _db.Events.FindAsync([ev.Id], ct);
            if (existing is null)
            {
                _db.Events.Add(EventRow.FromEvent(ev));
            }
            else
            {
                existing.Title = ev.Title;
                existing.Body = ev.Body;
                existing.StartDt = ev.StartDt;
                existing.EndDt = ev.EndDt;
                existing.IsAllDay = ev.IsAllDay;
                existing.NoticeDt = ev.NoticeDt;
                existing.ExternalLink = ev.ExternalLink;
                existing.CategoryId = ev.CategoryId;
                // IsFavorited 보존
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
