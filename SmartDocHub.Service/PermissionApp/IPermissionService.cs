using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.PermissionApp.Dto;

namespace SmartDocHub.Service.PermissionApp;

public interface IPermissionService
{
    Task<ServiceResult<Permission>> AddAsync(PermissionDto dto);

    Task<bool> DeleteAsync(long id);

    /// <param name="includeDisabled">是否包含已禁用权限，管理页需要</param>
    Task<List<Permission>> GetAllAsync(bool includeDisabled = true);

    Task<Permission?> GetAsync(long id);

    Task<ServiceResult> UpdateAsync(long id, PermissionUpdateDto dto);
}
