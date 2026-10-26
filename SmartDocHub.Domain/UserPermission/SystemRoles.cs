namespace SmartDocHub.Domain.UserPermission;

/// <summary>
/// 系统内置角色名常量。
/// 用于替代散落在各处的魔法字符串，避免角色改名或拼写不一致导致鉴权失效。
/// </summary>
public static class SystemRoles
{
    /// <summary>
    /// 超级管理员。该角色拥有全部权限，鉴权时直接放行。
    /// </summary>
    public const string SuperAdmin = "管理员";
}
