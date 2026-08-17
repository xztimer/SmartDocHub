using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.PermissionApp;

public class RolePermissionService(SmartDocHubDbContext _dbContext,IMemoryCache memoryCache) : IRolePermissionService, IBaseService
{
    public async Task<bool> SavePermissions(long roleId, List<long> permissionIds)
    {
        await _dbContext.RolePermissions.Where(t => t.RoleId == roleId)
            .ExecuteDeleteAsync();

        if (permissionIds == null || !permissionIds.Any())
        {
            return true;
        }

        var now = DateTime.Now;

        var newPermissions = permissionIds.Select(id => new RolePermissionMapping
        {
            RoleId = roleId,
            PermissionId = id
        });
        var roleName = await _dbContext.Roles.FirstOrDefaultAsync(t => t.Id == roleId);
        memoryCache.Remove($"ROLE_CODES_{roleName}");

        await _dbContext.RolePermissions.AddRangeAsync(newPermissions);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(long roleId)
    {
        var permissionIdList = await _dbContext.RolePermissions
            .Where(t => t.RoleId == roleId)
            .Select(t => t.PermissionId).ToListAsync();

        var permissionList = await _dbContext.Permissions
            .Where(t => permissionIdList.Contains(t.Id)).ToListAsync();

        return permissionList;
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(string roleName)
    {
        var role= await _dbContext.Roles
            .FirstOrDefaultAsync(t => t.Name == roleName && t.Status == RoleStatus.Normal);
        var permissionIdList = _dbContext.RolePermissions
            .Where(t => t.RoleId == role.Id)
            .Select(t => t.PermissionId).ToList();
        var permission = _dbContext.Permissions.Where(t => permissionIdList.Contains(t.Id)).ToList();
        return permission;

    }
}
