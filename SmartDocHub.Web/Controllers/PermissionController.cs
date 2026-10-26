using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Service.Common;
using SmartDocHub.Service.PermissionApp;
using SmartDocHub.Service.PermissionApp.Dto;
using SmartDocHub.Web.Auth;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 权限
/// </summary>
/// <param name="permissionService"></param>
[Route("api/[controller]")]
[ApiController]
public class PermissionController(IPermissionService permissionService) : ControllerBase
{
    /// <summary>
    /// 获取所有
    /// </summary>
    /// <param name="includeDisabled">是否包含已禁用权限，默认包含（管理页需要）</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeDisabled = true)
    {
        var res = await permissionService.GetAllAsync(includeDisabled);
        return Ok(res);
    }

    /// <summary>
    /// 单个
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("Get")]
    public async Task<IActionResult> Get(long id)
    {
        var res = await permissionService.GetAsync(id);
        return res is null ? NotFound("权限不存在") : Ok(res);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [HasPermission("system.permission.delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var isSuccess = await permissionService.DeleteAsync(id);
        if (isSuccess)
        {
            return NoContent();
        }
        return NotFound("未找到该权限");
    }

    /// <summary>
    /// 更新。以路由 id 为准，请求体不再直接绑定实体——
    /// 实体直绑配合整体覆盖会把客户端未传的字段写成默认值。
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [HasPermission("system.permission.edit")]
    public async Task<IActionResult> Update(long id, [FromBody] PermissionUpdateDto dto)
    {
        var result = await permissionService.UpdateAsync(id, dto);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Status == ServiceResultStatus.NotFound
            ? NotFound(result.Message)
            : BadRequest(result.Message);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [HasPermission("system.permission.add")]
    public async Task<IActionResult> Add([FromBody] PermissionDto dto)
    {
        var result = await permissionService.AddAsync(dto);
        if (!result.IsSuccess)
        {
            return result.Status == ServiceResultStatus.Conflict
                ? Conflict(result.Message)
                : BadRequest(result.Message);
        }

        return Created(string.Empty, result.Value);
    }
}
