using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp;
using SmartDocHub.Service.RoleApp;
using SmartDocHub.Service.RoleApp.Dto;
using SmartDocHub.Web.Auth;
using SmartDocHub.Web.Reponse;

namespace SmartDocHub.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController(IRoleService roleService,
    IMapper mapper, RoleManager<Role> roleManager,
    IRolePermissionService rolePermissionService,
    UserManager<User> userManager) : ControllerBase
{
    [HttpGet("All")]
    public async Task<IActionResult> GetAll()
    {
        var res = await roleService.GetAll();
        return Ok(res);
    }

    /// <summary>
    /// 分页搜索
    /// </summary>
    /// <param name="rolePageRequestDto"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] RolePageRequestDto rolePageRequestDto)
    {
        var rolePageResponseDto = await roleService.Query(rolePageRequestDto);
        return Ok(rolePageResponseDto);
    }

    /// <summary>
    /// 添加角色
    /// </summary>
    /// <param name="roleCreateDto"></param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission("system.role.add")]
    public async Task<IActionResult> Post([FromBody] RoleCreateDto roleCreateDto)
    {
        var role = mapper.Map<Role>(roleCreateDto);
        var result = await roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            var resultDto = mapper.Map<RoleDto>(role);
            return StatusCode(StatusCodes.Status201Created, resultDto);
        }
        else
        {
            var responseResult = new ResponseResultDto();
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            responseResult.SetError(errorMessages);
            return BadRequest(responseResult);
        }
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [HasPermission("system.role.delete")]
    public async Task<IActionResult> Delete(long id)
    {
        if (id == 1)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetError("系统内置初始角色，不可删除");
            return BadRequest(responseResult);
        }

        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetNotFound();
            return NotFound(responseResult);
        }
        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var responseResult = new ResponseResultDto();
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            responseResult.SetError(errorMessages);
            return BadRequest(responseResult);
        }

        return NoContent();
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="roleUpdateDto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [HasPermission("system.role.edit")]
    public async Task<IActionResult> Put(long id, [FromBody] RoleUpdateDto roleUpdateDto)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetNotFound();
            return NotFound(responseResult);
        }

        mapper.Map(roleUpdateDto, role);
        var result = await roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            return NoContent();
        }
        else
        {
            var responseResult = new ResponseResultDto();
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            responseResult.SetError(errorMessages);

            return BadRequest(responseResult);
        }
    }

    /// <summary>
    /// 角色详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(long id)
    {
        var rolePermissionDtos = await rolePermissionService.GetRolePermissionsAsync(id);
        return Ok(rolePermissionDtos);
    }

    /// <summary>
    /// 保存角色权限
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rolePermissionDto"></param>
    /// <returns></returns>
    [HttpPut("{id}/permissions")]
    [HasPermission("system.rolepermission.edit")]
    public async Task<IActionResult> SavePermissions(long id, [FromBody] RolePermissionDto rolePermissionDto)
    {
        var isSuccess = await rolePermissionService.SavePermissions(id, rolePermissionDto.PermissionIds);
        if (!isSuccess)
        {
            var responseResult = new ResponseResultDto();
            responseResult.SetError("保存角色权限失败，请重试");
            return BadRequest(responseResult);
        }
        return Ok();
    }

}
