using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.PermissionApp;

public interface IRolePermissionService
{
    Task<List<Permission>> GetRolePermissionsAsync(long roleId);
    Task<List<Permission>> GetRolePermissionsAsync(string roleName);
    Task<bool> SavePermissions(long roleId, List<long> permissionIds);
}
