using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SmartDocHub.Service.Exceptions;

namespace SmartDocHub.Web.Exception;

public class ExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is BusinessException businessException)
        {
            context.Result = new OkObjectResult(1);
            _logger.LogWarning($"业务异常触发: {businessException.Message}");
        }
        else
        {
            _logger.LogError(context.Exception, "系统发生未捕获的未预期异常");
            context.Result = new ObjectResult(1) { StatusCode = 500 };
        }

        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}
