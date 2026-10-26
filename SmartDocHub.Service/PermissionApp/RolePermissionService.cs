using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.PermissionApp;

public class RolePermissionService(SmartDocHubDbContext _dbContext, HybridCache cache)
    : IRolePermissionService, IBaseService
{
    public async Task<bool> SavePermissions(
        long roleId,
        List<long> permissionIds)
    {
        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == roleId);

        if (role is null)
        {
            return false;
        }

        // 先过滤掉不存在的权限 Id，避免进入事务后因外键失败导致权限被清空
        var requested = permissionIds?.Distinct().ToList() ?? new List<long>();
        List<long> validIds = requested.Count == 0
            ? new List<long>()
            : await _dbContext.Permissions
                .Where(p => requested.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

        // 删除与新增必须在同一事务内，否则中途失败会把角色权限清空
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            await _dbContext.RolePermissions
                .Where(t => t.RoleId == roleId)
                .ExecuteDeleteAsync();

            if (validIds.Count > 0)
            {
                await _dbContext.RolePermissions.AddRangeAsync(
                    validIds.Select(id => new RolePermissionMapping
                    {
                        RoleId = roleId,
                        PermissionId = id
                    }));

                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // 缓存键必须与鉴权侧一致，否则改权限后不生效
        await cache.RemoveAsync(CacheKeys.RolePermissionCodes(role.Name!));

        return true;
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(long roleId)
    {
        return await (from rp in _dbContext.RolePermissions
                      join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                      where rp.RoleId == roleId
                      select p)
                     .AsNoTracking()
                     .ToListAsync();
    }

    public async Task<List<Permission>> GetRolePermissionsAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return new List<Permission>();
        }

        // 角色或权限任一被禁用都必须立即失去授权，
        // 否则"禁用"只是一个不起作用的展示字段
        return await (from rp in _dbContext.RolePermissions
                      join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                      join r in _dbContext.Roles on rp.RoleId equals r.Id
                      where r.Name == roleName
                            && r.Status == RoleStatus.Normal
                            && p.Status == PermissionStatus.Normal
                      select p)
                     .AsNoTracking()
                     .ToListAsync();
    }

    /// <summary>
    /// 主动失效若干角色的权限码缓存。
    /// 权限本身被改/被删时，需要让所有持有它的角色重新取数。
    /// </summary>
    public async Task InvalidateAsync(IEnumerable<string> roleNames)
    {
        var keys = roleNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(CacheKeys.RolePermissionCodes)
            .Distinct()
            .ToList();

        if (keys.Count == 0)
        {
            return;
        }

        await cache.RemoveAsync(keys);
    }
}
