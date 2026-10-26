using Microsoft.AspNetCore.Authorization;

namespace SmartDocHub.Web.Auth;

/// <summary>
/// 动态权限要求
/// </summary>
/// <param name="code"></param>
public class RbacRequirement(string code) : IAuthorizationRequirement
{
    /// <summary>
    /// 权限code
    /// </summary>
    public string Code { get; } = code;
}
