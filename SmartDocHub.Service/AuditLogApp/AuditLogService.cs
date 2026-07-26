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

    public async Task<SysLog> AddAsync(SysLog sysLog)
    {
        try
        {
            var res = await _dbContext.SysLog.AddAsync(sysLog);
            await _dbContext.SaveChangesAsync();
            return res.Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "保存日志至数据库异常，接口：{0}\r\nMethod：{1}\r\n参数：{2}\r\nIP：{3}\r\n花费时长：{4}",
                   sysLog.RequestUrl, sysLog.Method, sysLog.RequestParam, sysLog.IP, sysLog.ExecutionTime);
            return sysLog;
        }
    }

    public async Task<AuditLogPageResponseDto> GetLogAsync(AuditLogPageRequestDto audiLogPageRequestDto)
    {
        int skip = audiLogPageRequestDto.PageSize * (audiLogPageRequestDto.PageIndex - 1);

        var query = _dbContext.SysLog.AsQueryable();
        var total = query.Count();
        if (!string.IsNullOrEmpty(audiLogPageRequestDto.RequestUrl))
        {
            query = query.Where(t => t.RequestUrl.Contains(audiLogPageRequestDto.RequestUrl));
        }
        if (!string.IsNullOrEmpty(audiLogPageRequestDto.IP))
        {
            query = query.Where(t => t.IP.Contains(audiLogPageRequestDto.IP));
        }
        if (audiLogPageRequestDto.StartTime != null)
        {
            query = query.Where(x => x.CreateTime >= audiLogPageRequestDto.StartTime);
        }
        if (audiLogPageRequestDto.EndTime != null)
        {
            query = query.Where(x => x.CreateTime <= audiLogPageRequestDto.EndTime);
        }
        if (audiLogPageRequestDto.AuditLogType != null)
        {
            query = query.Where(x => x.AuditLogType == audiLogPageRequestDto.AuditLogType);
        }

        var userIdList = await query.Select(t => t.UserId).Distinct().ToListAsync();
        var userDic = await _dbContext.Users
            .Where(t => userIdList.Contains(t.Id))
            .Select(t => new { t.Id, t.UserName }).ToDictionaryAsync(u => u.Id, u => u.UserName);
        var logList = await query.OrderByDescending(x => x.CreateTime).Skip(skip)
            .Take(audiLogPageRequestDto.PageSize).ToListAsync();

        var res = new AuditLogPageResponseDto
        {
            Total = total,
            PageIndex = audiLogPageRequestDto.PageIndex,
            PageSize = audiLogPageRequestDto.PageSize,
            AuditLogs = logList.Select(log => new SysLogDto
            {
                Id = log.Id,
                RequestUrl = log.RequestUrl,
                AuditLogType = log.AuditLogType,
                IP = log.IP,
                CreateTime = log.CreateTime,
                Creator = log.UserId.HasValue && userDic.TryGetValue(log.UserId.Value, out var name) ? name : null,
                ErrorMessage = log.ErrorMessage,
                ExecutionTime = log.ExecutionTime,
                Method = log.Method,
                RequestParam = log.RequestParam
            }).ToList()
        };
        return res;
    }
}
