namespace SmartDocHub.Domain.Doc;

public class Category
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string CategoryName { get; set; }
    public int Sort { get; set; }
    public CategoryStatus Status { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

}

public enum CategoryStatus
{
    Forbidden = 0,
    Normal = 1

}
