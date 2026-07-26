using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp;

namespace SmartDocHub.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionController(IPermissionService permissionService) : ControllerBase
{
    private readonly IPermissionService _permissionService = permissionService;

    [HttpGet("All")]
    public async Task<IActionResult> GetAll()
    {
        var res = await _permissionService.GetAllAsync();
        return Ok(res);
    }

    [HttpGet("Get")]
    public async Task<IActionResult> Get(long id)
    {
        var res = await _permissionService.GetAsync(id);
        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var isSuccess = await _permissionService.DeleteAsync(id);
        if (isSuccess)
        {
            return Ok("删除成功");
        }
        return BadRequest("删除失败");
    }

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

    [HttpPost]
    public async Task<IActionResult> Add(Permission permission)
    {
        var res = await _permissionService.AddAsync(permission);
        return Ok(res);
    }
}
