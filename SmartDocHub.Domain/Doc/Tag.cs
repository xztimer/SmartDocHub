namespace SmartDocHub.Domain.Doc;

public class Tag
{
    public long Id { get; set; }
    public string TagName { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
}
    