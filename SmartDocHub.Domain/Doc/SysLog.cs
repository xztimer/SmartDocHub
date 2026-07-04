namespace SmartDocHub.Domain.Doc;

public class SysLog
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string Action { get; set; }
    public string Method { get; set; }
    public string RequestUrl { get; set; }
    public string RequestParam { get; set; }
    public string ResponseResult { get; set; }
    public string IP { get; set; }
    public int ExecutionTime { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.UtcNow; 
}
