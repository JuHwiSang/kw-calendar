using KW_Calendar.Models;

namespace KW_Calendar.Services;

public class EventService : IEventService
{
    private readonly ILocalDbService _localDb;

    public EventService(ILocalDbService localDb) => _localDb = localDb;

    public Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
        => _localDb.GetEventsByDateRangeAsync(start, end, ct);

    public Task<Event?> GetEventByIdAsync(int id, CancellationToken ct = default)
        => _localDb.GetEventByIdAsync(id, ct);

    public async Task<Event> ToggleFavoriteAsync(int eventId, CancellationToken ct = default)
    {
        await _localDb.ToggleEventFavoriteAsync(eventId, ct);
        var ev = await _localDb.GetEventByIdAsync(eventId, ct);
        return ev ?? throw new InvalidOperationException($"Event {eventId} not found after toggle.");
    }

    public Task<IReadOnlyList<Event>> GetFavoritedEventsAsync(CancellationToken ct = default)
        => _localDb.GetFavoritedEventsAsync(ct);
}
