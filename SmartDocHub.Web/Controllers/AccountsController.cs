using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp;
using SmartDocHub.Service.UserApp.Dto;
using SmartDocHub.Web.Auth;
using SmartDocHub.Web.Reponse;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 账号控制器
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountsController(UserManager<User> userManager,
    IRolePermissionService rolePermissionService) : ControllerBase
{

    /// <summary>
    /// 获取个人信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userName = User.Identity?.Name;
        var user = await userManager.FindByNameAsync(userName);
        var roles = await userManager.GetRolesAsync(user);

        var accountDto = new AccountDto()
        {
            Roles = roles.ToArray(),
            Name = userName,
            Avatar = "",
            Introduction = "这家伙什么都没说……"
        };

        return Ok(accountDto);
    }

    /// <summary>
    /// 获取授权列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetError("无法获取当前登录用户信息");
            return Unauthorized(responseResult);
        }
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetError("用户不存在或已被删除");
            return Unauthorized(responseResult);
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles == null || !roles.Any())
        {
            return Ok(new List<Permission>());
        }
        var permissionTasks = roles.Select(rolePermissionService.GetRolePermissionsAsync);
        var permissionsPerRole = await Task.WhenAll(permissionTasks);

        var permissions = permissionsPerRole
            .SelectMany(permissions => permissions)
            .DistinctBy(permission => permission.Id)
            .ToList();
;

        return Ok(permissions);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="userUpdatePwdDto"></param>
    /// <returns></returns>
    [HttpPost("password")]
    public async Task<IActionResult> UpdatePwd([FromBody] AccountUpdatePwdDto userUpdatePwdDto)
    {
        var userName = User.Identity?.Name;
        var user = await userManager.FindByNameAsync(userName);
        var result = await userManager.ChangePasswordAsync(user, userUpdatePwdDto.OldPassword, userUpdatePwdDto.NewPassword);

        if (result.Succeeded)
        {
            return Ok();
        }
        else
        {
            var responseResult = new ResponseResultDto();
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            responseResult.SetError(errorMessages);

            return BadRequest(responseResult);
        }
    }

}
