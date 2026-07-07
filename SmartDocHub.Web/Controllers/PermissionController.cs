using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp;

namespace SmartDocHub.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

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

    [HttpPost("Delete")]
    public IActionResult Delete(long id)
    {
        var isSuccess = _permissionService.Delete(id);
        if (isSuccess)
        {
            return Ok("删除成功");
        }
        return BadRequest("删除失败");
    }

    [HttpPost("Update")]
    public async Task<IActionResult> UpdateAsync(Permission permission)
    {
        var isSuccess = await _permissionService.UpdateAsync(permission);
        if (isSuccess)
        {
            return Ok();
        }
        return BadRequest();
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddAsync(Permission permission)
    {
        var res = await _permissionService.AddAsync(permission);
        return Ok(res);
    }
}
