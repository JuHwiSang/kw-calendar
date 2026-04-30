namespace KW_Calendar.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // true이면 해당 카테고리의 모든 이벤트를 즐겨찾기로 취급
    public bool IsFavorited { get; set; }
}
