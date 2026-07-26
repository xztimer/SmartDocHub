using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;

namespace SmartDocHub.Service.PermissionApp;

public class PermissionService(SmartDocHubDbContext dbContext, IMemoryCache memoryCache) : IPermissionService
{
    public bool HasPermission(long userId, string permissionCode)
    {
        var permissions = GetUserPermissions(userId);
        return permissions.Contains(permissionCode);
    }

    public List<string> GetUserPermissions(long userId)
    {
        var cacheKey = $"perm:{userId}";
        var cache = memoryCache.Get<List<string>>(cacheKey);
        if (cache != null)
        {
            return cache;
        }

        var roleIds = dbContext.UserRoleMappings
            .Where(t => t.UserId == userId)
            .Select(t => t.RoleId)
            .ToList();

        var permissionIds = dbContext.RolePermissions
            .Where(t => roleIds.Contains(t.RoleId))
            .Select(t => t.PermissionId)
            .ToList();
        var perms = dbContext.Permissions
            .Where(t => permissionIds.Contains(t.Id))
            .Select(t => t.Code).Distinct().ToList();
        return perms;
    }

    public async Task<Permission> AddAsync(Permission permission)
    {
        var isExist = dbContext.Set<Permission>().Any(t => t.Code == permission.Code);
        if (isExist)
        {
            return null;
        }
        dbContext.Set<Permission>().Add(permission);
        await dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        int rowsAffected = await dbContext.Permissions
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync();

        return rowsAffected > 0;
    }
    public async Task<Permission> GetAsync(long id)
    {
        var item = await dbContext.Set<Permission>().FirstOrDefaultAsync(t => t.Id == id);
        return item;
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        var res = await dbContext.Set<Permission>().ToListAsync();
        return res;
    }

    public async Task<bool> UpdateAsync(Permission permission)
    {
        var isExist = await dbContext.Set<Permission>().AnyAsync(t => t.Id == permission.Id);
        if (!isExist)
        {
            return false;
        }
        dbContext.Update(permission);
        await dbContext.SaveChangesAsync();
        return true;
    }


}
