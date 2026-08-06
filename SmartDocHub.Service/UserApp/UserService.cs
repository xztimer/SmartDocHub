using AutoMapper;

using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.RoleApp.Dto;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public class UserService(SmartDocHubDbContext dbContext, IMapper mapper) : IUserService, IBaseService
{
    public async Task<UserPageResponseDto> GetPagedListAsync(UserPageRequestDto request)
    {

        var query = dbContext.Users.Where(t => !t.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(request.UserName))
        {
            query = query.Where(t => t.UserName.Contains(request.UserName));
        }
        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.CreateTime)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var userRoleMappings = await dbContext.UserRoles
            .Where(t => userIds.Contains(t.UserId))
            .Select(t => new { t.UserId, t.RoleId })
            .ToListAsync();
        var roleIds = userRoleMappings.Select(t=>t.RoleId).Distinct().ToList();
        var roles = await dbContext.Roles.Where(t => roleIds.Contains(t.Id)).ToListAsync();
        var roleDtoDict = mapper.Map<List<RoleDto>>(roles).ToDictionary(r => r.Id); ;
    
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
        var userPageResponseDto = new UserPageResponseDto
        {
            Total = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            Users = items
        };
        return userPageResponseDto;
    }


    public List<UserAllDto> GetAll()
    {
        var list = dbContext.Users.ToList();

        return mapper.Map<List<UserAllDto>>(list);
    }

}
