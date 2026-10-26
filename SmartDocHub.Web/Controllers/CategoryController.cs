using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Service.CategoryApp;
using SmartDocHub.Service.CategoryApp.Dto;
using SmartDocHub.Service.Common;
using SmartDocHub.Web.Auth;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 分类
/// </summary>
/// <param name="categoryService"></param>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>
    /// 分类列表（不含已删除）
    /// </summary>
    /// <param name="includeDisabled">是否包含已禁用分类，默认包含（管理页需要）</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeDisabled = true)
    {
        var categories = await categoryService.GetListAsync(includeDisabled);
        return Ok(categories);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var result = await categoryService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result.Status, result.Message);
        }

        return Created(string.Empty, new { id = result.Value });
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [HasPermission("system.category.edit")]
    public async Task<IActionResult> Update(long id, [FromBody] CategoryUpdateDto dto)
    {
        var result = await categoryService.UpdateAsync(id, dto);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Status, result.Message);
    }

    /// <summary>
    /// 删除（软删除）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [HasPermission("system.category.delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await categoryService.DeleteAsync(id);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Status, result.Message);
    }

    private IActionResult ToErrorResult(ServiceResultStatus status, string message) => status switch
    {
        ServiceResultStatus.NotFound => NotFound(message),
        ServiceResultStatus.Conflict => Conflict(message),
        _ => BadRequest(message)
    };
}
