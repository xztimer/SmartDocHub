using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Service.AuditLogApp.Dto;

namespace SmartDocHub.Service.AuditLogApp;

public interface IAuditLogService
{
    Task<SysLog> AddAsync(SysLog sysLog);
    Task<AuditLogPageResponseDto> GetLogAsync(AuditLogPageRequestDto audiLogPageRequestDto);
}
