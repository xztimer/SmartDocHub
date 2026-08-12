using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Service.AuditLogApp;

using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace SmartDocHub.Web.AuditLog;

/// <summary>
/// 审计日志过滤器
/// </summary>
public class AuditLogFilter(ILogger<AuditLogFilter> logger, AuditLogQueue auditLogQueue) : IAsyncActionFilter
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var auditLogAttribute = context.ActionDescriptor
            .EndpointMetadata
            .OfType<AuditLogAttribute>().FirstOrDefault();
        if (auditLogAttribute != null && !auditLogAttribute.IsOpen)
        {
            await next();
            return;
        }

        var sw = Stopwatch.StartNew();
        var sysLog = CreateBaseLog(context);
        var executedContext = await next();
        sw.Stop();

        sysLog.ExecutionTime = (int)sw.ElapsedMilliseconds;
        sysLog.ResponseResult = GetResponseResultString(executedContext.Result);

        if (executedContext.Exception != null && !executedContext.ExceptionHandled)
        {
            HandleException(sysLog, executedContext.Exception);
        }

        auditLogQueue.QueueLog(sysLog);
    }

    private static SysLog CreateBaseLog(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        long? userId = null;
        var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out var parsedId))
        {
            userId = parsedId;
        }

        return new SysLog
        {
            UserId = userId,
            RequestUrl = request.Path,
            Method = request.Method,
            AuditLogType = AuditLogType.Info,
            IP = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty,
            RequestParam = SerializeArguments(context.ActionArguments)
        };
    }

    private void HandleException(SysLog sysLog, Exception exception)
    {
        sysLog.ErrorMessage = exception.Message.Length > 1024
            ? exception.Message[..1024]
            : exception.Message;
        sysLog.Error = exception.ToString();
        sysLog.AuditLogType = AuditLogType.Exception;

        logger.LogError(exception, "接口异常：{Url} | Method：{Method} | 参数：{Param} | IP：{IP} | 耗时：{Time}ms",
            sysLog.RequestUrl, sysLog.Method, sysLog.RequestParam, sysLog.IP, sysLog.ExecutionTime);
    }

    private static string SerializeArguments(IDictionary<string, object?> arguments)
    {
        if (arguments == null || arguments.Count == 0) return string.Empty;

        var safeArguments = arguments
            .Where(x => x.Value is not IFormFile
                     && x.Value is not Stream
                     && x.Value is not CancellationToken)
            .ToDictionary(x => x.Key, x => x.Value);

        try
        {
            return JsonSerializer.Serialize(safeArguments, _jsonOptions);
        }
        catch (Exception ex)
        {
            return $"[序列化参数失败]: {ex.Message}";
        }
    }

    private static string GetResponseResultString(IActionResult? result)
    {
        if (result == null) return string.Empty;

        object? dataToSerialize = result switch
        {
            ObjectResult objectResult => objectResult.Value,
            JsonResult jsonResult => jsonResult.Value,
            ContentResult contentResult => contentResult.Content,
            _ => null
        };

        if (dataToSerialize == null) return string.Empty;
        if (dataToSerialize is string str) return str;

        try
        {
            return JsonSerializer.Serialize(dataToSerialize, _jsonOptions);
        }
        catch (Exception ex)
        {
            return $"[序列化返回值失败]: {ex.Message}";
        }
    }
}