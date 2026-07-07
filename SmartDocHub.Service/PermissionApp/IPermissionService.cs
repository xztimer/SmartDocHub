using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.PermissionApp;

public interface IPermissionService
{
    Task<Permission> AddAsync(Permission permission);
    bool Delete(long id);
    Task<List<Permission>> GetAllAsync();
    Task<Permission> GetAsync(long id);
    Task<bool> UpdateAsync(Permission permission);
}
