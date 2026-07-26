namespace SmartDocHub.Domain.Doc;

public class Feedback
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public long UserId { get; set; }

    // 查询是否为层中回复
    public long? ParentId { get; set; }

    // 回复人id
    public int? ReplyToUserId { get; set; }
    public int Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public FeedbackStatus Status { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

}

public enum FeedbackStatus
{
    待审核 = 0,
    已发布 = 1,
    已隐藏 = 2
}
