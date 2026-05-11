using KW_Calendar.Models;

namespace KW_Calendar.Services;

public interface ISupabaseService
{
    Task<IReadOnlyList<Event>> FetchEventsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> FetchCategoriesAsync(CancellationToken ct = default);
}
