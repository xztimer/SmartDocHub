using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;
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
        var roleNamesDict = await GetUserRoleNamesDictAsync(userIds);

        List<UserDto>? items = users.Select(u =>
        {
            var dto = mapper.Map<UserDto>(u);
            dto.RoleNames = roleNamesDict.GetValueOrDefault(u.Id, new List<string>());
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

    public async Task<UserDto?> GetAsync(long id)
    {
        var user = await dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return null;

        var dto = mapper.Map<UserDto>(user);

        var roleNamesDict = await GetUserRoleNamesDictAsync(new List<long> { id });
        dto.RoleNames = roleNamesDict.GetValueOrDefault(id, new List<string>());

        return dto;
    }

    private async Task<Dictionary<long, List<string>>> GetUserRoleNamesDictAsync(List<long> userIds)
    {
        var mappings = await dbContext.UserRoles
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync();

        var roleIds = mappings.Select(m => m.RoleId).Distinct().ToList();
        var roleDict = await dbContext.Set<Role>()
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        return mappings.GroupBy(m => m.UserId)
            .ToDictionary(g => g.Key, g => g.Select(m => roleDict.GetValueOrDefault(m.RoleId, "")).Where(n => !string.IsNullOrEmpty(n)).ToList());
    }

}
