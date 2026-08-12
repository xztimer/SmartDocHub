using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Service.DepartmentApp;
using SmartDocHub.Service.DepartmentApp.Dto;
using SmartDocHub.Web.Auth;

namespace SmartDocHub.Web.Controllers;

/// <summary>
/// 部门控制器
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DepartmentController(IDepartmentService departmentService) : ControllerBase
{
    /// <summary>
    /// 获取单条数据
    /// </summary>
    [HttpGet("Get")]
    public async Task<IActionResult> Get(long id)
    {
        var res = await departmentService.GetAsync(id);
        return Ok(res);
    }

    /// <summary>
    /// 查询列表
    /// </summary>
    [HttpGet("GetList")]
    public async Task<IActionResult> GetList([FromQuery] string? key)
    {
        var res = await departmentService.GetListAsync(key);
        return Ok(res);
    }

    /// <summary>
    /// 获取所有部门
    /// </summary>
    [HttpGet("All")]
    public async Task<IActionResult> GetAll()
    {
        var res = await departmentService.GetAllAsync();
        return Ok(res);
    }

    /// <summary>
    /// 创建
    /// </summary>
    [HttpPost("Add")]
    [HasPermission("system.department.add")]
    public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
    {
        var result = await departmentService.CreateAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }
        return Created(string.Empty, result.Entity);
    }

    /// <summary>
    /// 更新
    /// </summary>
    [HttpPost("Update")]
    [HasPermission("system.department.edit")]
    public async Task<IActionResult> Update([FromBody] DepartmentUpdateDto dto)
    {
        var result = await departmentService.UpdateAsync(dto);
        if (!result.IsSuccess)
        {
            // 根据具体的业务错误类型，可以微调返回 NotFound 还是 BadRequest
            if (result.Message == "部门不存在") return NotFound(result.Message);
            return BadRequest(result.Message);
        }
        return Ok();
    }

    /// <summary>
    /// 删除
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("system.department.delete")]
    public async Task<IActionResult> Delete([FromQuery] long id)
    {
        var result = await departmentService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Message == "要删除的部门不存在") return NotFound(result.Message);
            return BadRequest(result.Message);
        }
        return Ok();
    }
}