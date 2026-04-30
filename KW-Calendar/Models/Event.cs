namespace KW_Calendar.Models;

public class Event
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
    // 클라이언트 전용 필드 — 서버 DB에 없음
    public bool IsFavorited { get; set; }
}
