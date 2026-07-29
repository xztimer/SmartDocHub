using SmartDocHub.Domain.AuditLog;

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocHub.Service.AuditLogApp.Dto;

public class SysLogDto
{
    public long Id { get; set; }
    public string Creator { get; set; }
    public string Method { get; set; }
    public string RequestUrl { get; set; }
    public string RequestParam { get; set; }
    public string ResponseResult { get; set; }
    public string IP { get; set; }
    public int ExecutionTime { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string CreateTime { get; set; }
    public AuditLogType AuditLogType { get; set; }
}
