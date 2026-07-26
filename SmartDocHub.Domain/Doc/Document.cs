namespace SmartDocHub.Domain.Doc;

public class Document
{
    public long Id { get; set; }
    public string Title { get; set; } 

    public string Content { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public int UploaderId { get; set; }
    public int DepId { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateTime { get; set; }
}
