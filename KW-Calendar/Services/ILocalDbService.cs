using KW_Calendar.Models;

namespace KW_Calendar.Services;

/// <summary>
/// 클라이언트 SQLite DB에 대한 모든 읽기/쓰기를 담당하는 어댑터.
/// EventService, CategoryService 등은 이 인터페이스에만 의존하며 직접 DB에 접근하지 않는다.
/// </summary>
public interface ILocalDbService
{
    // --- Events ---

    Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<Event?> GetEventByIdAsync(int id, CancellationToken ct = default);

    /// <summary>서버에서 받아온 이벤트를 upsert. 클라이언트 전용 필드(IsFavorited)는 기존 값 보존. IsUserAdded 행은 건드리지 않는다.</summary>
    Task UpsertEventsAsync(IEnumerable<Event> events, CancellationToken ct = default);

    /// <summary>유저가 직접 작성한 일정을 INSERT. IsFavorited=true, IsUserAdded=true로 강제 설정.</summary>
    Task AddUserEventAsync(Event e, CancellationToken ct = default);

    /// <summary>유저가 직접 작성한 일정을 삭제. IsUserAdded=false인 경우 예외.</summary>
    Task DeleteUserEventAsync(int eventId, CancellationToken ct = default);

    /// <summary>유저가 직접 작성한 일정의 편집 가능 필드를 업데이트. IsUserAdded=false인 경우 예외.</summary>
    Task UpdateUserEventAsync(Event e, CancellationToken ct = default);

    Task<IReadOnlyList<Event>> GetFavoritedEventsAsync(CancellationToken ct = default);

    /// <summary>IsFavorited를 토글하고 변경된 값을 반환.</summary>
    Task<bool> ToggleEventFavoriteAsync(int eventId, CancellationToken ct = default);

    // --- Categories ---

    Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task UpsertCategoriesAsync(IEnumerable<Category> categories, CancellationToken ct = default);

    /// <summary>IsFavorited를 토글하고 변경된 값을 반환.</summary>
    Task<bool> ToggleCategoryFavoriteAsync(int categoryId, CancellationToken ct = default);

    /// <summary>알림 발송 완료 상태를 기록. null 전달 시 해당 필드 변경 안 함.</summary>
    Task MarkEventNotificationsAsync(int eventId, bool? oneDayBefore, bool? sameDay, CancellationToken ct = default);
}
