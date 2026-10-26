using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Service.AuditLogApp;

using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartDocHub.Web.AuditLog;

/// <summary>
/// 审计日志过滤器
/// </summary>
public class AuditLogFilter(ILogger<AuditLogFilter> logger, AuditLogQueue auditLogQueue) : IAsyncActionFilter
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    /// <summary>
    /// 需要脱敏的字段名。审计日志会落库并对外展示，口令类字段一律不得明文保存。
    /// </summary>
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "oldPassword", "newPassword", "confirmPassword",
        "token", "accessToken", "refreshToken", "secret", "securityKey"
    };

    private const string MaskedValue = "***";

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

        ActionExecutedContext executedContext;
        try
        {
            executedContext = await next();
        }
        catch (Exception exception)
        {
            // 异常若由更外层的过滤器接管，next() 会直接抛出。
            // 这条路径同样必须落库，否则该请求不会留下任何审计记录。
            sw.Stop();
            sysLog.ExecutionTime = (int)sw.ElapsedMilliseconds;
            HandleException(sysLog, exception);
            auditLogQueue.QueueLog(sysLog);
            throw;
        }

        sw.Stop();
        sysLog.ExecutionTime = (int)sw.ElapsedMilliseconds;
        sysLog.ResponseResult = GetResponseResultString(executedContext.Result);

        // 全局异常过滤器已把 ExceptionHandled 置为 true，按 !ExceptionHandled 判断会让
        // AuditLogType.Exception 永远不可达；这里只判断是否发生过异常，且异常只在此处落库一次。
        if (executedContext.Exception is not null)
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
            .ToDictionary(x => x.Key, x => MaskSensitive(x.Value));

        try
        {
            return JsonSerializer.Serialize(safeArguments, _jsonOptions);
        }
        catch (Exception ex)
        {
            return $"[序列化参数失败]: {ex.Message}";
        }
    }

    /// <summary>
    /// 递归脱敏：命中敏感字段名则替换为掩码，其余字段保持原样。
    /// </summary>
    private static object? MaskSensitive(object? value)
    {
        if (value is null) return null;

        try
        {
            var node = JsonSerializer.SerializeToNode(value, value.GetType(), _jsonOptions);
            MaskNode(node);
            return node;
        }
        catch
        {
            // 无法结构化时退回原值，由外层序列化兜底
            return value;
        }
    }

    private static void MaskNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(p => p.Key).ToList())
                {
                    if (SensitiveKeys.Contains(key))
                    {
                        obj[key] = MaskedValue;
                    }
                    else
                    {
                        MaskNode(obj[key]);
                    }
                }
                break;

            case JsonArray array:
                foreach (var item in array)
                {
                    MaskNode(item);
                }
                break;
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