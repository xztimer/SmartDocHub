using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Service.AuditLogApp.Dto;

namespace SmartDocHub.Service.AuditLogApp;

public interface IAuditLogService
{
    Task AddRangeAsync(List<SysLog> sysLogs);
    Task<AuditLogPageResponseDto> GetLogAsync(AuditLogPageRequestDto audiLogPageRequestDto);
}
