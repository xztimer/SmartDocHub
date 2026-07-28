namespace SmartDocHub.Domain.AuditLog;

public class SysLog
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Method { get; set; }
    public string RequestUrl { get; set; }
    public string RequestParam { get; set; }
    public string ResponseResult { get; set; }
    public string IP { get; set; }
    public int ExecutionTime { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public AuditLogType AuditLogType { get; set; }
}


public enum AuditLogType
{
    Info,
    Exception = 99
}