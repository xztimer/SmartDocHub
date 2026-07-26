using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.AuditLogApp.Dto;

public class AuditLogPageResponseDto : PageResponseDto
{
    public List<SysLogDto>? AuditLogs { get; set; }
}
