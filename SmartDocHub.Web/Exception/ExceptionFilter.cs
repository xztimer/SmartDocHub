using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SmartDocHub.Service.Exceptions;
using SmartDocHub.Web.Reponse;

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
            var res = new ApiResult<object>(businessException.Code, businessException.Message);
            context.Result = new OkObjectResult(res);
            _logger.LogWarning($"业务异常触发: {businessException.Message}");
        }
        else
        {
            _logger.LogError(context.Exception, "系统发生未捕获的未预期异常");
            var res = new ApiResult<object>(ResponseCode.ServerError, "服务器发生了未知错误");
            context.Result = new ObjectResult(res) { StatusCode = 500 };
        }

        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}
