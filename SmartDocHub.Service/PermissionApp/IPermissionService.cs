using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp.Dto;

namespace SmartDocHub.Service.PermissionApp;

public interface IPermissionService
{
    Task<Permission> AddAsync(PermissionDto dto);
    Task<bool> DeleteAsync(long id);
    Task<List<Permission>> GetAllAsync();
    Task<Permission> GetAsync(long id);
    List<string> GetUserPermissions(long userId);
    bool HasPermission(long userId, string permissionCode);
    Task<bool> UpdateAsync(Permission permission);
}
