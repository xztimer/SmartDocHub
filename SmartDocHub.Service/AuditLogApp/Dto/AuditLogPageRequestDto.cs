using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Service.Common;

using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocHub.Service.AuditLogApp.Dto;

public class AuditLogPageRequestDto : PageRequestDto
{
    public long Id { get; set; }
    public string RequestUrl { get; set; }
    public string IP { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public AuditLogType AuditLogType { get; set; }
}
