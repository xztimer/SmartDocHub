using Microsoft.EntityFrameworkCore.Diagnostics;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Service.AuditLogApp;

namespace SmartDocHub.Web.AuditLog;

/// <summary>
/// 日志消费托管服务
/// </summary>
/// <param name="auditLogQueue"></param>
/// <param name="serviceProvider"></param>
/// <param name="logger"></param>
public class AuditLogConsumerService(
    AuditLogQueue auditLogQueue,
    IServiceProvider serviceProvider,
    ILogger<AuditLogConsumerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batchList = new List<SysLog>();

        while (await auditLogQueue.Reader.WaitToReadAsync(stoppingToken))
        {
            while (auditLogQueue.Reader.TryRead(out var sysLog))
            {
                batchList.Add(sysLog);
            }

            if (batchList.Count > 0)
            {
                await FlushLogsAsync(batchList);
            }
        }
    }

    private async Task FlushLogsAsync(List<SysLog> sysLogs)
    {
        if (sysLogs.Count == 0) return;
        try
        {
            using var scoped = serviceProvider.CreateScope();
            var auditLogService = scoped.ServiceProvider.GetRequiredService<IAuditLogService>();
            await auditLogService.AddRangeAsync(sysLogs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "异步审计日志写入数据库失败，本次丢失日志数：{Count}", sysLogs.Count);
        }
        finally
        {
            sysLogs.Clear();
        }
    }
}
