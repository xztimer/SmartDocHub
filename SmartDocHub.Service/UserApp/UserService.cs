using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public class UserService : IUserService, IBaseService
{
    private readonly SmartDocHubDbContext _dbContext;
    private readonly IMapper _mapper;

    public UserService(SmartDocHubDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<UserPageResponseDto> GetPagedListAsync(UserPageRequestDto request)
    {

        var query = _dbContext.Users.AsQueryable();
        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.CreateTime)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var deptIds = users.Where(u => u.DeptId.HasValue).Select(u => u.DeptId!.Value).Distinct().ToList();
        var deptDict = await _dbContext.Set<Department>()
            .Where(d => deptIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.DeptName);

        var userIds = users.Select(u => u.Id).ToList();
        var roleNamesDict = await GetUserRoleNamesDictAsync(userIds);

        var items = users.Select(u =>
        {
            var dto = _mapper.Map<UserDto>(u);
            dto.DeptName = u.DeptId.HasValue && deptDict.TryGetValue(u.DeptId.Value, out var name) ? name : null;
            dto.RoleNames = roleNamesDict.GetValueOrDefault(u.Id, new List<string>());
            return dto;
        }).ToList();

        return new UserPageResponseDto();
    }


    public List<UserAllDto> GetAll()
    {
        var list = _dbContext.Users.ToList();

        return _mapper.Map<List<UserAllDto>>(list);
    }

    public async Task<UserDto?> GetAsync(long id)
    {
        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return null;

        var dto = _mapper.Map<UserDto>(user);

        if (user.DeptId.HasValue)
        {
            var dept = await _dbContext.Set<Department>().FirstOrDefaultAsync(d => d.Id == user.DeptId.Value);
            dto.DeptName = dept?.DeptName;
        }

        var roleNamesDict = await GetUserRoleNamesDictAsync(new List<long> { id });
        dto.RoleNames = roleNamesDict.GetValueOrDefault(id, new List<string>());

        return dto;
    }

    public async Task<bool> UpdateAsync(UserUpdateDto dto)
    {
        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (user == null)
            return false;

        user.NickName = dto.NickName;
        user.DeptId = dto.DeptId;
        user.Remark = dto.Remark;
        user.Status = dto.Status;
        user.UpdateTime = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, dto.Password);
        }

        _dbContext.Set<User>().Update(user);
        await _dbContext.SaveChangesAsync();

        if (dto.RoleNames != null)
        {
            await AssignRolesAsync(dto.Id, dto.RoleNames);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return false;

        var userRoleMappings = await _dbContext.Set<UserRoleMapping>()
            .Where(x => x.UserId == id)
            .ToListAsync();
        _dbContext.Set<UserRoleMapping>().RemoveRange(userRoleMappings);

        _dbContext.Set<User>().Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRolesAsync(long userId, List<string> roleNames)
    {
        var existing = await _dbContext.Set<UserRoleMapping>()
            .Where(x => x.UserId == userId)
            .ToListAsync();
        _dbContext.Set<UserRoleMapping>().RemoveRange(existing);

        var roles = await _dbContext.Set<Role>()
            .Where(r => roleNames.Contains(r.Name))
            .ToListAsync();

        var mappings = roles.Select(r => new UserRoleMapping
        {
            UserId = userId,
            RoleId = r.Id
        });
        _dbContext.Set<UserRoleMapping>().AddRange(mappings);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task<Dictionary<long, List<string>>> GetUserRoleNamesDictAsync(List<long> userIds)
    {
        var mappings = await _dbContext.Set<UserRoleMapping>()
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync();

        var roleIds = mappings.Select(m => m.RoleId).Distinct().ToList();
        var roleDict = await _dbContext.Set<Role>()
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        return mappings.GroupBy(m => m.UserId)
            .ToDictionary(g => g.Key, g => g.Select(m => roleDict.GetValueOrDefault(m.RoleId, "")).Where(n => !string.IsNullOrEmpty(n)).ToList());
    }
}
