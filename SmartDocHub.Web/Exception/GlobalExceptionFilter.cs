using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartDocHub.Web.Exception;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled == false)
        {
            var str = $"异常：{context.HttpContext.Request.Path}{context.Exception.Message}";
            logger.LogWarning(str);
            context.Result = new ObjectResult("服务器内部错误，请联系管理员")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
