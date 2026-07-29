using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Reponse;

using System.Security.Claims;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
/// <param name="userService"></param>
/// <param name="userManager"></param>
/// <param name="mapper"></param>
[Route("api/[controller]")]
[ApiController]
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
        user.NormalizedUserName = user.UserName;
        user.CreateTime = DateTime.UtcNow;

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, dto.Password);

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetError("创建失败，请检查用户账号是否重复");
            return BadRequest(responseResult);
        }
        return Created(string.Empty, mapper.Map<UserDto>(user));
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UserUpdateDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound("该用户不存在");
        }

        mapper.Map(dto, user);
        user.UpdateTime = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, dto.Password);
            if (!resetResult.Succeeded)
            {
                var passwordErrors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                return BadRequest($"密码更新失败: {passwordErrors}");
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return BadRequest($"用户信息更新失败: {errors}");
        }

        if (dto.RoleNames != null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(dto.RoleNames).ToList();
            var rolesToAdd = dto.RoleNames.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    return BadRequest("移除旧角色失败");
                }
            }

            if (rolesToAdd.Any())
            {
                var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    return BadRequest("添加新角色失败");
                }
            }
        }
        return NoContent();
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        if (id == 1)
        {
            return BadRequest("初始的用户，不可删除");
        }

        User? user = await userManager.FindByIdAsync(id.ToString());
        if (user == null || user.IsDeleted)
        {
            return NotFound("未找到该用户或用户已被删除");
        }
        user.IsDeleted = true;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest("删除用户失败：" + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return NoContent();
    }

    /// <summary>
    /// 用户状态
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> ToggleStatus(long id)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(currentUserIdStr, out long currentUserId) && id == currentUserId)
        {
            return BadRequest("不能修改当前登录账号的状态");
        }
        if (id == 1)
        {
            return BadRequest("初始管理员账号状态不可变更");
        }

        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound("未找到该用户");
        }
        var targetStatus = user.Status == UserStatus.Normal ? UserStatus.Forbidden : UserStatus.Normal;

        user.Status = targetStatus;
        user.DisabledTime = targetStatus == UserStatus.Forbidden ? DateTime.UtcNow : null;

        if (targetStatus == UserStatus.Forbidden)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest("更新状态失败");
        }

        return Ok(new { UserId = user.Id, user.Status });
    }

}
