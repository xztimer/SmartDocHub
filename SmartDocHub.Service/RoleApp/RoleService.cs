using AutoMapper;

using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.RoleApp.Dto;

namespace SmartDocHub.Service.RoleApp;

public class RoleService : IRoleService
{
    private readonly SmartDocHubDbContext _dbContext;
    private readonly IMapper _mapper;

    public RoleService(SmartDocHubDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<List<RoleDto>> GetAllRole()
    {
        var roleList = await _dbContext.Set<Role>().ToListAsync();
        var res = _mapper.Map<List<RoleDto>>(roleList);
        return res;
    }

    
}
