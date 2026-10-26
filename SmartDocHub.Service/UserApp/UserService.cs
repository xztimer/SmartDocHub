using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.RoleApp.Dto;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public class UserService(
    SmartDocHubDbContext dbContext,
    IMapper mapper,
    UserManager<User> userManager) : IUserService, IBaseService
{
    public async Task<UserPageResponseDto> GetPagedListAsync(UserPageRequestDto request)
    {

        var query = dbContext.Users.Where(t => !t.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(request.UserName))
        {
            query = query.Where(t => t.UserName.Contains(request.UserName));
        }
        if (request.DeptId.HasValue)
        {
            query = query.Where(t => t.DeptId == request.DeptId);
        }
        var total = await query.CountAsync();

        // CreateTime 可能重复（批量导入、种子数据），必须用主键兜底才能保证翻页稳定
        var users = await query
            .OrderBy(u => u.CreateTime)
            .ThenBy(u => u.Id)
            .Skip(request.GetSkipCount())
            .Take(request.GetPageSize())
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var userRoleMappings = await dbContext.UserRoles
            .Where(t => userIds.Contains(t.UserId))
            .Select(t => new { t.UserId, t.RoleId })
            .ToListAsync();
        var roleIds = userRoleMappings.Select(t => t.RoleId).Distinct().ToList();
        var roles = await dbContext.Roles.Where(t => roleIds.Contains(t.Id)).ToListAsync();
        var roleDtoDict = mapper.Map<List<RoleDto>>(roles).ToDictionary(r => r.Id); ;
        var deptIds = users.Select(t => t.DeptId).ToList();



        var userRolesDict = userRoleMappings
            .GroupBy(m => m.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(m => roleDtoDict.TryGetValue(m.RoleId, out var roleDto) ? roleDto : null)
                      .Where(r => r != null)
                      .Cast<RoleDto>()
                      .ToList()
            );

        List<UserDto>? items = users.Select(u =>
        {
            var dto = mapper.Map<UserDto>(u);
            dto.Roles = userRolesDict.TryGetValue(u.Id, out var userRoles)
        ? userRoles
        : new List<RoleDto>();
            return dto;
        }).ToList();
        var deptDic = await dbContext.Departments
            .Where(t => deptIds.Contains(t.Id))
            .AsNoTracking()
            .Select(t => new
            {
                t.Id,
                t.DeptName
            })
            .ToDictionaryAsync(t => t.Id, t => t.DeptName);
        foreach (var item in items)
        {
            item.DeptName = deptDic.GetValueOrDefault(item.DeptId);
        }

        var userPageResponseDto = new UserPageResponseDto
        {
            Total = total,
            PageIndex = request.PageIndex,
            PageSize = request.GetPageSize(),
            Users = items
        };
        return userPageResponseDto;
    }


    public async Task<List<UserAllDto>> GetAllAsync()
    {
        var list = await dbContext.Users.AsNoTracking()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Id)
            .ToListAsync();

        return mapper.Map<List<UserAllDto>>(list);
    }

    /// <summary>
    /// 更新用户资料、口令与角色。
    /// 三者必须整体成功或整体回滚，避免出现"提示失败但资料已改、角色未改"的中间态。
    /// </summary>
    public async Task<ServiceResult> UpdateAsync(long id, UserUpdateDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null || user.IsDeleted)
        {
            return ServiceResult.NotFound("该用户不存在");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            mapper.Map(dto, user);
            user.UpdateTime = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, dto.Password);
                if (!resetResult.Succeeded)
                {
                    return ServiceResult.Fail($"密码更新失败: {JoinErrors(resetResult)}");
                }
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return ServiceResult.Fail($"用户信息更新失败: {JoinErrors(updateResult)}");
            }

            if (dto.RoleNames is not null)
            {
                var currentRoles = await userManager.GetRolesAsync(user);

                var rolesToRemove = currentRoles.Except(dto.RoleNames).ToList();
                var rolesToAdd = dto.RoleNames.Except(currentRoles).ToList();

                if (rolesToRemove.Count > 0)
                {
                    var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        return ServiceResult.Fail("移除旧角色失败");
                    }
                }

                if (rolesToAdd.Count > 0)
                {
                    var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        return ServiceResult.Fail("添加新角色失败");
                    }
                }
            }

            await transaction.CommitAsync();
            return ServiceResult.Ok();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string JoinErrors(IdentityResult result)
        => string.Join("; ", result.Errors.Select(e => e.Description));
}
