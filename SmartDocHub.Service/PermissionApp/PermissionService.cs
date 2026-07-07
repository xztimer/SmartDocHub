using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;

namespace SmartDocHub.Service.PermissionApp;

public class PermissionService : IPermissionService
{
    private readonly SmartDocHubDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;

    public PermissionService(SmartDocHubDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public bool HasPermission(long userId, string permissionCode)
    {
        var permissions = GetUserPermissions(userId);
        return permissions.Contains(permissionCode);
    }

    public List<string> GetUserPermissions(long userId)
    {
        var cacheKey = $"perm:{userId}";

        var cache = _memoryCache.Get<List<string>>(cacheKey);
        if (cache != null)
        {
            return cache;
        }

        var roleIds = _dbContext.Set<UserRoleMapping>()
            .Where(t => t.UserId == userId)
            .Select(t => t.RoleId)
            .ToList();

        var permissionIds = _dbContext.Set<RolePermissionMapping>()
            .Where(t => roleIds.Contains(t.RoleId))
            .Select(t => t.PermissionId)
            .ToList();
        var perms = _dbContext.Set<Permission>()
            .Where(t => permissionIds.Contains(t.Id))
            .Select(t => t.Code).Distinct().ToList();
        return perms;
    }

    public async Task<Permission> AddAsync(Permission permission)
    {
        var isExist = _dbContext.Set<Permission>().Any(t => t.Code == permission.Code);
        if (isExist)
        {
            return null;
        }
        _dbContext.Set<Permission>().Add(permission);
        await _dbContext.SaveChangesAsync();
        return permission;
    }

    public bool Delete(long id)
    {
        var entity = _dbContext.Set<Permission>().FirstOrDefault(t => t.Id == id);
        if (entity == null)
        {
            return false;
        }

        _dbContext.Remove(entity);

        return true;
    }

    public async Task<Permission> GetAsync(long id)
    {
        var item = await _dbContext.Set<Permission>().FirstOrDefaultAsync(t => t.Id == id);
        return item;
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        var res = await _dbContext.Set<Permission>().ToListAsync();

        return res;
    }

    public async Task<bool> UpdateAsync(Permission permission)
    {
        var isExist = await _dbContext.Set<Permission>().AnyAsync(t => t.Id == permission.Id);
        if (!isExist)
        {
            return false;
        }
        _dbContext.Update(permission);
        await _dbContext.SaveChangesAsync();
        return true;
    }


}
