using Microsoft.AspNetCore.Authorization;

namespace SmartDocHub.Web.Auth;

/// <summary>
/// 封装权限特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string code) => Policy = code;
}
