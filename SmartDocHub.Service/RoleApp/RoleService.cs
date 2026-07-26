using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.RoleApp.Dto;

namespace SmartDocHub.Service.RoleApp;

public class RoleService(
    SmartDocHubDbContext _dbContext,
    IMapper _mapper) : IRoleService
{

    public async Task<List<RoleDto>> GetAll()
    {
        var list = await _dbContext.Roles.AsNoTracking().ToListAsync();
        var roleDtos = _mapper.Map<List<RoleDto>>(list);
        return roleDtos;
    }

    public async Task<RolePageResponseDto> Query(RolePageRequestDto rolePageRequestDto)
    {
        var query = _dbContext.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(rolePageRequestDto.Name))
        {
            query = query.Where(x => x.Name.Contains(rolePageRequestDto.Name));
        }

        var count = await query.CountAsync();
        var skip = (rolePageRequestDto.PageIndex - 1) * rolePageRequestDto.PageSize;
        var list = await query.Skip(skip).Take(rolePageRequestDto.PageSize).ToListAsync();

        var result = new RolePageResponseDto
        {
            PageIndex = rolePageRequestDto.PageIndex,
            PageSize = rolePageRequestDto.PageSize,
            Total = count,
            Roles = _mapper.Map<List<RoleDto>>(list)
        };

        return result;
    }
}
