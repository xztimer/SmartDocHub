using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Service.AuditLogApp;
using SmartDocHub.Service.AuditLogApp.Dto;
using SmartDocHub.Web.AuditLog;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 日志控制器
/// </summary>
/// <param name="auditLogService"></param>
[Route("api/[controller]")]
[ApiController]
public class AuditLogController(IAuditLogService auditLogService) : ControllerBase
{
    private readonly IAuditLogService _auditLogService = auditLogService;

   /// <summary>
   /// 日志列表
   /// </summary>
   /// <param name="auditLogPageRequestDto"></param>
   /// <returns></returns>
    [HttpGet]
    [AuditLog(IsOpen = false)]
    public async Task<IActionResult> GetAuditLogList(AuditLogPageRequestDto auditLogPageRequestDto)
    {
        var res = await _auditLogService.GetLogAsync(auditLogPageRequestDto);
        return Ok(res);
    }
}
