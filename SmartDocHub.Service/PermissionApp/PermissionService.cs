using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.PermissionApp.Dto;

namespace SmartDocHub.Service.PermissionApp;

public class PermissionService(SmartDocHubDbContext dbContext, IMemoryCache memoryCache, IMapper mapper) : IPermissionService
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

        var roleIds = dbContext.UserRoles
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

    public async Task<Permission> AddAsync(PermissionDto dto)
    {
        var isExist = await dbContext.Permissions.AnyAsync(t => t.Code == dto.Code);
        if (isExist)
        {
            return null;
        }
        var permission = mapper.Map<Permission>(dto);
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        int rowsAffected = await dbContext.Permissions.Where(t => t.Id == id).ExecuteDeleteAsync();
        return rowsAffected > 0;
    }
    public async Task<Permission> GetAsync(long id)
    {
        var item = await dbContext.Permissions.FirstOrDefaultAsync(t => t.Id == id);
        return item;
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        var res = await dbContext.Permissions
            .Where(t => t.Status == PermissionStatus.Normal).ToListAsync();
        return res;
    }

    public async Task<bool> UpdateAsync(Permission permission)
    {
        var isExist = await dbContext.Permissions.AnyAsync(t => t.Id == permission.Id);
        if (!isExist)
        {
            return false;
        }
        dbContext.Update(permission);
        await dbContext.SaveChangesAsync();
        return true;
    }


}
