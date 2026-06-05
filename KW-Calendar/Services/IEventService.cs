using KW_Calendar.Models;

namespace KW_Calendar.Services;

public interface IEventService
{
    Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<Event?> GetEventByIdAsync(int id, CancellationToken ct = default);
    Task<Event> ToggleFavoriteAsync(int eventId, CancellationToken ct = default);
    // 이벤트 즐겨찾기 + 카테고리 즐겨찾기 모두 병합, StartDt 오름차순 정렬
    Task<IReadOnlyList<Event>> GetFavoritedEventsAsync(CancellationToken ct = default);

    Task AddUserEventAsync(Event e, CancellationToken ct = default);
    Task DeleteUserEventAsync(int eventId, CancellationToken ct = default);
    Task UpdateUserEventAsync(Event e, CancellationToken ct = default);
}
