using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using SmartDocHub.Service.PermissionApp;
using SmartDocHub.Web.Reponse;

namespace SmartDocHub.Web.Auth;

/// <summary>
/// 权限校验特性 — 标注在 Controller 或 Action 上，指定访问该接口所需的权限码
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _code;

    /// <summary>
    /// 指定访问所需的权限码
    /// </summary>
    /// <param name="code">权限编码，对应 Permission.Code 字段</param>
    public PermissionAttribute(string code)
    {
        _code = code;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 如果前面的过滤器已经设置了结果（如被短路），则跳过
        if (context.Result != null) return;

        // 提取用户ID
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(
                new ResponseResultDto().SetError("未登录或Token无效"));
            return;
        }

        // 从 DI 容器获取权限服务并校验
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        var hasPermission = await permissionService.HasPermissionAsync(userId, _code);

        if (!hasPermission)
        {
            context.Result = new ObjectResult(new ResponseResultDto().SetNoPermission())
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
