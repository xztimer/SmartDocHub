//using AutoMapper;

//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//using SmartDocHub.Domain.UserPermission;
//using SmartDocHub.Infrastructure;
//using SmartDocHub.Service.DepartmentApp;
//using SmartDocHub.Service.DepartmentApp.Dto;

//namespace SmartDocHub.Web.Controllers;

///// <summary>
///// 部门控制器
///// </summary>
///// <param name="dbContext"></param>
///// <param name="mapper"></param>
//[Route("api/[controller]")]
//[ApiController]
//public class DepartmentController(SmartDocHubDbContext dbContext, IMapper mapper) : ControllerBase
//{
//    /// <summary>
//    /// 获取单条数据
//    /// </summary>
//    /// <param name="id"></param>
//    /// <returns></returns>
//    [HttpGet("Get")]
//    public async Task<IActionResult> Get(long id)
//    {
//        var res = await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
//        return Ok(res);
//    }

//    /// <summary>
//    /// 查询列表
//    /// </summary>
//    /// <param name="key"></param>
//    /// <returns></returns>
//    [HttpGet("GetList")]
//    public async Task<IActionResult> GetList([FromQuery] string? key)
//    {
//        var allDepts = await dbContext.Departments
//            .Include(d => d.Children)
//            .Where(d => !d.IsDeleted)
//            .OrderBy(d => d.Sort)
//            .ToListAsync();
//        if (!string.IsNullOrWhiteSpace(key))
//        {
//            var matched = allDepts
//            .Where(d => d.DeptName.Contains(key) || (d.Code != null && d.Code.Contains(key)))
//            .ToList();

//            var resultSet = new HashSet<Department>();
//            foreach (var item in matched)
//            {
//                var cur = item;
//                while (cur != null)
//                {
//                    resultSet.Add(cur);
//                    // 向上找到父节点
//                    cur = allDepts.FirstOrDefault(d => d.Id == cur.ParentId);
//                }
//            }
//            var resultIds = resultSet.Select(x => x.Id).ToHashSet();
//            var searchRoots = resultSet
//                .Where(d => d.ParentId == null || d.ParentId == 0 || !resultIds.Contains(d.ParentId.Value))
//                .OrderBy(d => d.Sort)
//                .ToList();

//            return Ok(searchRoots);
//        }

//        var rootNodes = allDepts
//            .Where(d => d.ParentId == null || d.ParentId == 0)
//            .OrderBy(d => d.Sort)
//            .ToList();

//        return Ok(rootNodes);
//    }


//    /// <summary>
//    /// 创建
//    /// </summary>
//    /// <param name="dto"></param>
//    /// <returns></returns>
//    [HttpPost("Add")]
//    public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
//    {
//        if (dto.ParentId == 0) dto.ParentId = null;

//        var hasConflict = await dbContext.Departments.AnyAsync(t =>
//            !t.IsDeleted &&
//            t.ParentId == dto.ParentId &&
//            (t.DeptName == dto.DeptName || (!string.IsNullOrEmpty(dto.Code) && t.Code == dto.Code)));

//        if (hasConflict)
//        {
//            return BadRequest("同级下已存在相同的部门名称或部门编码");
//        }

//        var entity = mapper.Map<Department>(dto);

//        if (entity.Sort == 0)
//        {
//            var maxSort = await dbContext.Departments
//                .Where(t => !t.IsDeleted && t.ParentId == entity.ParentId)
//                .Select(t => (int?)t.Sort)
//                .MaxAsync();
//            entity.Sort = (maxSort ?? 0) + 1;
//        }

//        await dbContext.Departments.AddAsync(entity);
//        await dbContext.SaveChangesAsync();

//        return Created(string.Empty, entity);
//    }

//    /// <summary>
//    /// 更新
//    /// </summary>
//    /// <param name="dto"></param>
//    /// <returns></returns>
//    [HttpPost("Update")]
//    public async Task<IActionResult> Update([FromBody] DepartmentUpdateDto dto)
//    {
//        var entity = await dbContext.Departments
//            .Include(d => d.Children)
//            .FirstOrDefaultAsync(d => d.Id == dto.Id && !d.IsDeleted);

//        if (entity == null) return NotFound("部门不存在");

//        if (dto.ParentId == dto.Id)
//        {
//            return BadRequest("上级部门不能选自己");
//        }

//        var hasConflict = await dbContext.Departments.AnyAsync(t =>
//            !t.IsDeleted &&
//            t.Id != dto.Id &&
//            t.ParentId == dto.ParentId &&
//            (t.DeptName == dto.DeptName || (!string.IsNullOrEmpty(dto.Code) && t.Code == dto.Code)));

//        if (hasConflict)
//        {
//            return BadRequest("同级下已存在相同的部门名称或部门编码");
//        }

//        mapper.Map(dto, entity);
//        await dbContext.SaveChangesAsync();

//        return Ok();
//    }

//    /// <summary>
//    /// 删除
//    /// </summary>
//    /// <param name="id"></param>
//    /// <returns></returns>
//    [HttpDelete("Delete")]
//    public async Task<IActionResult> Delete([FromQuery] long id)
//    {
//        var department = await dbContext.Departments
//            .Include(d => d.Children.Where(c => !c.IsDeleted))
//            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
//        if (department == null)
//        {
//            return NotFound("要删除的部门不存在");
//        }
//        if (department.Children.Any())
//        {
//            return BadRequest("该部门下仍存在子部门，请先删除或转移子部门");
//        }
//        var hasUser = await dbContext.Users.AnyAsync(t => t.DeptId == id);
//        if (hasUser)
//        {
//            return BadRequest("该部门存在用户，不能删除");
//        }

//        department.IsDeleted = true;
//        await dbContext.SaveChangesAsync();

//        return Ok();
//    }
//}
