using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Reponse;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
/// <param name="userService"></param>
/// <param name="userManager"></param>
/// <param name="mapper"></param>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(IUserService userService, UserManager<User> userManager, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// 用户列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("List")]
    public async Task<IActionResult> GetList([FromQuery] UserPageRequestDto request)
    {
        var res = await userService.GetPagedListAsync(request);
        return Ok(res);
    }

    /// <summary>
    /// 查询用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Get")]
    public async Task<IActionResult> Get(long id)
    {
        var res = await userService.GetAsync(id);
        if (res == null)
            return NotFound();
        return Ok(res);
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] UserCreateDto dto)
    {
        var user = mapper.Map<User>(dto);
        user.EmailConfirmed = true;
        user.SecurityStamp = DateTime.UtcNow.Ticks.ToString();

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, dto.Password);

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetError("创建失败，请检查用户账号是否重复");
            return BadRequest(responseResult);
        }

        if (dto.RoleNames is { Count: > 0 })
        {
            await userService.AssignRolesAsync(user.Id, dto.RoleNames);
        }

        return Created(string.Empty, mapper.Map<UserDto>(user));
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
    {
        var success = await userService.UpdateAsync(dto);
        if (!success)
            return NotFound();
        return Ok();
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await userService.DeleteAsync(id);
        if (!success)
            return NotFound();
        return Ok();
    }
}
