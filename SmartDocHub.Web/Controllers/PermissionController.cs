using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp;
using SmartDocHub.Service.PermissionApp.Dto;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 权限
/// </summary>
/// <param name="permissionService"></param>
[Route("api/[controller]")]
[ApiController]
public class PermissionController(IPermissionService permissionService) : ControllerBase
{
    private readonly IPermissionService _permissionService = permissionService;

    /// <summary>
    /// 获取所有
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<IActionResult> GetAll()
    {
        var res = await _permissionService.GetAllAsync();
        return Ok(res);
    }

   /// <summary>
   /// 单个
   /// </summary>
   /// <param name="id"></param>
   /// <returns></returns>
    [HttpGet("Get")]
    public async Task<IActionResult> Get(long id)
    {
        var res = await _permissionService.GetAsync(id);
        return Ok(res);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var isSuccess = await _permissionService.DeleteAsync(id);
        if (isSuccess)
        {
            return NoContent();
        }
        return NotFound("未找到该权限");
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Permission permission)
    {
        var isSuccess = await _permissionService.UpdateAsync(permission);
        if (isSuccess)
        {
            return Ok();
        }
        return BadRequest();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody]PermissionDto dto)
    {
        var res = await _permissionService.AddAsync(dto);
        return Ok(res);
    }
}
