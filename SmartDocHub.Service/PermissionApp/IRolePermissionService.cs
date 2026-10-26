using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.PermissionApp;

public interface IRolePermissionService
{
    Task<List<Permission>> GetRolePermissionsAsync(long roleId);
    Task<List<Permission>> GetRolePermissionsAsync(string roleName);
    Task<bool> SavePermissions(long roleId, List<long> permissionIds);

    /// <summary>主动失效若干角色的权限码缓存</summary>
    Task InvalidateAsync(IEnumerable<string> roleNames);
}
