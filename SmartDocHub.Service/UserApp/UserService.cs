using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Exceptions;
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

    public async Task<User> CheckLogin(LoginDto loginDto)
    {

        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(t=>t.UserName== loginDto.UserName);
        if(user is null)
        {
            throw new BusinessException("用户名或密码错误");
        }
        var ph = new PasswordHasher<User>();
        var res = ph.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
        if (res == PasswordVerificationResult.Failed)
        {
            throw new BusinessException("用户名或密码错误");
        }

        if (res == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = ph.HashPassword(user, loginDto.Password);
            await _dbContext.SaveChangesAsync();
        }

        return user;

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
