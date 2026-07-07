using Microsoft.AspNetCore.Mvc;

using SmartDocHub.Service.RoleApp;

namespace SmartDocHub.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }


}
