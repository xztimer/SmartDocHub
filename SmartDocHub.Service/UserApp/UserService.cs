using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public class UserService : IUserService,IBaseService
{
    private readonly SmartDocHubDbContext _dbContext;
    private readonly IMapper _mapper;

    public UserService(SmartDocHubDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<bool> CheckLogin(LoginDto loginDto)
    {

        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(t=>t.UserName== loginDto.UserName);
        if(user is null)
        {
            
        }
    }


    public async Task<User> CreateUser(UserCreateDto userCreateDto)
    {
        var user = _mapper.Map<User>(userCreateDto);
        PasswordHasher<User> ph = new PasswordHasher<User>();
        
        user.PasswordHash = ph.HashPassword(user, "123456");
        user.Status = UserStatus.Normal;
        var entity = _dbContext.Set<User>().Add(user).Entity;
        await _dbContext.SaveChangesAsync();
        return entity;
    }
}
