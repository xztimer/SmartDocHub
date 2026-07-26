using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.PermissionApp;

public interface IPermissionService
{
    Task<Permission> AddAsync(Permission permission);
    Task<bool> DeleteAsync(long id);
    Task<List<Permission>> GetAllAsync();
    Task<Permission> GetAsync(long id);
    List<string> GetUserPermissions(long userId);
    bool HasPermission(long userId, string permissionCode);
    Task<bool> UpdateAsync(Permission permission);
}
