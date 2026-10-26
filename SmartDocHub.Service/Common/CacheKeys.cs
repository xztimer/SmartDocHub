namespace SmartDocHub.Service.Common;

/// <summary>
/// 缓存键集中定义。
/// 规范：模块:业务:标识。禁止在业务代码中散落字符串插值，避免"写入键"与"失效键"不一致。
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// 角色拥有的权限码集合。以角色名为标识（令牌中携带的也是角色名）。
    /// </summary>
    public static string RolePermissionCodes(string roleName) => $"rbac:role:{roleName}:codes";

    /// <summary>
    /// 用户令牌状态（SecurityStamp + 账号状态），用于令牌校验短缓存。
    /// </summary>
    public static string UserTokenState(long userId) => $"token:state:{userId}";
}
