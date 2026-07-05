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
            .Select(t=>t.Code).Distinct().ToList();
        return perms;
    }
}
