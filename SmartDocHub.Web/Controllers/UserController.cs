using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.UserApp;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Auth;
using SmartDocHub.Web.Reponse;

using System.Security.Claims;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
/// <param name="userService"></param>
/// <param name="userManager"></param>
/// <param name="mapper"></param>
/// <param name="tokenStatusValidator"></param>
[Route("api/[controller]")]
[ApiController]
public class UserController(
    IUserService userService,
    UserManager<User> userManager,
    IMapper mapper,
    ITokenStatusValidator tokenStatusValidator) : ControllerBase
{
    /// <summary>
    /// 用户列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("List")]
    public async Task<IActionResult> GetList([FromQuery] UserPageRequestDto request)
    {
        UserPageResponseDto? res = await userService.GetPagedListAsync(request);
        return Ok(res);
    }

    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("Add")]
    [HasPermission("system.user.add")]
    public async Task<IActionResult> Add([FromBody] UserCreateDto dto)
    {
        var user = mapper.Map<User>(dto);
        user.EmailConfirmed = true;
        user.CreateTime = DateTime.UtcNow;

        // 交给 UserManager 统一校验口令强度并生成规范化字段，避免绕过口令策略
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var responseResult = new ResponseResultDto();
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            responseResult.SetError($"创建失败：{errorMessages}");
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
    [HasPermission("system.user.edit")]
    public async Task<IActionResult> Update(long id, [FromBody] UserUpdateDto dto)
    {
        var result = await userService.UpdateAsync(id, dto);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Status == ServiceResultStatus.NotFound
            ? NotFound(result.Message)
            : BadRequest(result.Message);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [HasPermission("system.user.delete")]
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

        // 立即失效该用户已签发的令牌
        tokenStatusValidator.Invalidate(id);

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

        // 状态变更立即生效，不等 30 秒状态缓存过期
        tokenStatusValidator.Invalidate(user.Id);

        return Ok(new { UserId = user.Id, user.Status });
    }

}
