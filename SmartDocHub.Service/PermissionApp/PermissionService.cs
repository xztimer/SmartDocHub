using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.PermissionApp.Dto;

namespace SmartDocHub.Service.PermissionApp;

public class PermissionService(
    SmartDocHubDbContext dbContext,
    IRolePermissionService rolePermissionService) : IPermissionService, IBaseService
{
    public async Task<ServiceResult<Permission>> AddAsync(PermissionDto dto)
    {
        var code = dto.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            return ServiceResult<Permission>.Fail("权限编码不能为空");
        }

        var isExist = await dbContext.Permissions.AnyAsync(t => t.Code == code);
        if (isExist)
        {
            // 重复必须是 409，原实现返回 200 + 空 body，前端无法区分成功与失败
            return ServiceResult<Permission>.Conflict($"权限编码 {code} 已存在");
        }

        var permission = new Permission
        {
            ParentId = dto.ParentId,
            Name = dto.Name?.Trim() ?? string.Empty,
            Code = code,
            Type = dto.Type,
            Path = dto.Path,
            ApiMethod = dto.ApiMethod,
            Icon = dto.Icon,
            Status = dto.Status
        };

        dbContext.Permissions.Add(permission);
        await dbContext.SaveChangesAsync();

        return ServiceResult<Permission>.Ok(permission);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        // 删除前先记下受影响的角色，删除后必须让这些角色的权限缓存失效，
        // 否则被删掉的权限码在缓存里还能继续放行最多 20 分钟
        var roleNames = await GetRoleNamesByPermissionAsync(id );

        int rowsAffected = await dbContext.Permissions.Where(t => t.Id == id).ExecuteDeleteAsync();
        if (rowsAffected == 0)
        {
            return false;
        }

        await rolePermissionService.InvalidateAsync(roleNames );
        return true;
    }

    public async Task<Permission?> GetAsync(long id)
    {
        return await dbContext.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id );
    }

    public async Task<List<Permission>> GetAllAsync(bool includeDisabled = true)
    {
        var query = dbContext.Permissions.AsNoTracking().AsQueryable();
        if (!includeDisabled)
        {
            query = query.Where(t => t.Status == PermissionStatus.Normal);
        }

        return await query.OrderBy(t => t.Id).ToListAsync();
    }

    public async Task<ServiceResult> UpdateAsync(long id, PermissionUpdateDto dto)
    {
        var permission = await dbContext.Permissions.FirstOrDefaultAsync(t => t.Id == id);
        if (permission is null)
        {
            return ServiceResult.NotFound("权限不存在");
        }

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Fail("权限名称不能为空");
        }

        if (dto.ParentId == id)
        {
            return ServiceResult.Fail("上级权限不能是自己");
        }

        // 逐字段赋值，不整体覆盖：客户端没传的字段必须保持原值
        permission.ParentId = dto.ParentId;
        permission.Name = name;
        permission.Type = dto.Type;
        permission.Path = dto.Path;
        permission.ApiMethod = dto.ApiMethod;
        permission.Icon = dto.Icon;
        if (dto.Status.HasValue)
        {
            permission.Status = dto.Status.Value;
        }

        await dbContext.SaveChangesAsync();

        // 状态、路径等变更同样影响鉴权结果，缓存必须失效
        var roleNames = await GetRoleNamesByPermissionAsync(id );
        await rolePermissionService.InvalidateAsync(roleNames );

        return ServiceResult.Ok();
    }

    private Task<List<string>> GetRoleNamesByPermissionAsync(long permissionId  )
        => (from rp in dbContext.RolePermissions
            join r in dbContext.Roles on rp.RoleId equals r.Id
            where rp.PermissionId == permissionId && r.Name != null
            select r.Name!)
           .Distinct()
           .ToListAsync();
}
