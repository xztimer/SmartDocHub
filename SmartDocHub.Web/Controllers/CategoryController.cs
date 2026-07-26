using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.Doc;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.CategoryApp.Dto;

namespace SmartDocHub.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(SmartDocHubDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// 分类列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await dbContext.Categories.ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var parentExists = await dbContext.Categories.AnyAsync(c => c.Id == dto.ParentId);
        if (!parentExists)
        {
            return BadRequest("指定的父级分类不存在");
        }

        var category = new Category
        {
            CategoryName = dto.Name,
            ParentId = dto.ParentId
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        return Created();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category == null) return NotFound("分类不存在");
        if (dto.ParentId == id) return BadRequest("父级分类不能是自己");


        var parentExists = await dbContext.Categories.AnyAsync(c => c.Id == dto.ParentId);
        if (!parentExists) return BadRequest("指定的父级分类不存在");


        category.CategoryName = dto.Name;
        category.ParentId = dto.ParentId;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category == null) return NotFound();

        var hasChildren = await dbContext.Categories.AnyAsync(c => c.ParentId == id);
        if (hasChildren)
        {
            return BadRequest("该分类下存在子分类，请先删除或转移子分类");
        }
        var hasDocuments = await dbContext.Documents.AnyAsync(d => d.CategoryId == id);
        if (hasDocuments)
        {
            return BadRequest("该分类下存在关联的文章，无法直接删除");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

}
