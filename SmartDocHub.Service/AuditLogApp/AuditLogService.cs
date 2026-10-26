using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.AuditLogApp.Dto;
using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.AuditLogApp;

public class AuditLogService : IAuditLogService, IBaseService
{
    private readonly SmartDocHubDbContext _dbContext;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(SmartDocHubDbContext dbContext,
        ILogger<AuditLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AddRangeAsync(List<SysLog> sysLogs)
    {
        if (sysLogs == null || sysLogs.Count == 0) return;

        await _dbContext.SysLog.AddRangeAsync(sysLogs);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AuditLogPageResponseDto> GetLogAsync(AuditLogPageRequestDto audiLogPageRequestDto)
    {
        var query = _dbContext.SysLog.AsQueryable();
        if (!string.IsNullOrEmpty(audiLogPageRequestDto.RequestUrl))
        {
            query = query.Where(t => t.RequestUrl.Contains(audiLogPageRequestDto.RequestUrl));
        }
        if (!string.IsNullOrEmpty(audiLogPageRequestDto.IP))
        {
            query = query.Where(t => t.IP.Contains(audiLogPageRequestDto.IP));
        }

        if (audiLogPageRequestDto.StartTime is { } startTime)
        {
            var start = DateTimeUtc.AsUtc(startTime);
            query = query.Where(x => x.CreateTime >= start);
        }
        if (audiLogPageRequestDto.EndTime is { } endTime)
        {
            var end = DateTimeUtc.AsUtc(endTime);
            query = query.Where(x => x.CreateTime <= end);
        }
        if (audiLogPageRequestDto.AuditLogType != null)
        {
            query = query.Where(x => x.AuditLogType == audiLogPageRequestDto.AuditLogType);
        }

        int total = await query.CountAsync();

        // CreateTime 可能同毫秒重复，用主键兜底排序，否则翻页会重复或漏行
        var logList = await query
            .OrderByDescending(x => x.CreateTime)
            .ThenByDescending(x => x.Id)
            .Skip(audiLogPageRequestDto.GetSkipCount())
            .Take(audiLogPageRequestDto.GetPageSize())
            .ToListAsync();

        // 只查当前页涉及的用户，避免为整个过滤结果集做 DISTINCT
        var userIdList = logList
            .Where(t => t.UserId.HasValue)
            .Select(t => t.UserId!.Value)
            .Distinct()
            .ToList();
        var userDic = await _dbContext.Users
            .Where(t => userIdList.Contains(t.Id))
            .Select(t => new { t.Id, t.UserName }).ToDictionaryAsync(u => u.Id, u => u.UserName);

        var res = new AuditLogPageResponseDto
        {
            Total = total,
            PageIndex = audiLogPageRequestDto.PageIndex,
            PageSize = audiLogPageRequestDto.GetPageSize(),
            AuditLogs = logList.Select(log => new SysLogDto
            {
                Id = log.Id,
                RequestUrl = log.RequestUrl,
                AuditLogType = log.AuditLogType,
                IP = log.IP,
                CreateTime = DateTimeUtc.ToIsoString(log.CreateTime),
                Creator = log.UserId.HasValue && userDic.TryGetValue(log.UserId.Value, out var name) ? name : null,
                ErrorMessage = log.ErrorMessage,
                Error = log.Error,
                ExecutionTime = log.ExecutionTime,
                Method = log.Method,
                RequestParam = log.RequestParam
            }).ToList()
        };
        return res;
    }
}
