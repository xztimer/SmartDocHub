using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.AuditLogApp;

using System.Diagnostics;
using System.Text.Json;

namespace SmartDocHub.Web.AuditLog;

/// <summary>
/// 日志过滤器
/// </summary>
public class AuditLogFilter(ILogger<ExceptionFilterAttribute> logger, AuditLogQueue auditLogQueue) : IAsyncActionFilter
{

    /// <summary>
    /// 实现方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var httpContext = context.HttpContext;

        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<User>>();
        var sysLog = await GenerateRequestLogAsync(context, userManager);

        var executedContext = await next();

        sw.Stop();
        sysLog.ExecutionTime = (int)sw.ElapsedMilliseconds;
        sysLog.ResponseResult = GetResponseResultString(executedContext.Result);

        if (executedContext.Exception != null && !executedContext.ExceptionHandled)
        {
            System.Exception? exception = executedContext.Exception;
            sysLog.ErrorMessage = exception.Message.Length > 1024
                ? exception.Message[..1024]
                : exception.Message;
            sysLog.Error = exception.ToString();

            sysLog.AuditLogType = AuditLogType.Exception;

            logger.LogError(exception, "接口：{Url}\r\nMethod：{Method}\r\n参数：{Param}\r\nIP：{IP}\r\n花费时长：{Time}ms",
                sysLog.RequestUrl, sysLog.Method, sysLog.RequestParam, sysLog.IP, sysLog.ExecutionTime);
        }
        var auditLogAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<AuditLogAttribute>()
            .FirstOrDefault();
        if (auditLogAttribute == null || auditLogAttribute.IsOpen)
        {
            auditLogQueue.QueueLog(sysLog);
        }
    }

    private async Task<SysLog> GenerateRequestLogAsync(ActionExecutingContext context, UserManager<User> userManager)
    {
        var log = new SysLog();
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        var userName = httpContext.User.Identity?.Name;
        if (!string.IsNullOrEmpty(userName))
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user != null)
            {
                log.UserId = user.Id;
            }
        }

        log.RequestUrl = request.Path;
        log.Method = request.Method;
        log.AuditLogType = AuditLogType.Info;
        log.IP = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;

        log.RequestParam = SerializeArguments(context.ActionArguments);

        return log;
    }

    private string SerializeArguments(IDictionary<string, object?> arguments)
    {
        if (arguments == null || arguments.Count == 0) return string.Empty;
        var safeArguments = arguments
            .Where(x => x.Value is not IFormFile
                     && x.Value is not Stream
                     && x.Value is not CancellationToken)
            .ToDictionary(x => x.Key, x => x.Value);

        return JsonSerializer.Serialize(safeArguments);
    }

    private string GetResponseResultString(IActionResult? result)
    {
        if (result == null) return string.Empty;

        object? dataToSerialize = result switch
        {
            ObjectResult objectResult => objectResult.Value,
            JsonResult jsonResult => jsonResult.Value,
            ContentResult contentResult => contentResult.Content,
            _ => result.ToString()
        };

        return dataToSerialize == null ? string.Empty : JsonSerializer.Serialize(dataToSerialize);
    }
}
