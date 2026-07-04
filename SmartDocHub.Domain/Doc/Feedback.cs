namespace SmartDocHub.Domain.Doc;

public class Feedback
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public long UserId { get; set; }
    public int Score { get; set; }
    public string Content { get; set; }
    public FeedbackStatus Status { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime UpdateTime { get; set; }

}

public enum FeedbackStatus
{
    待审核 = 0,
    已发布 = 1,
    已隐藏 = 2
}
