using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.Doc;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.CategoryApp.Dto;
using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.CategoryApp;

public class CategoryService(SmartDocHubDbContext dbContext) : ICategoryService, IBaseService
{
    public async Task<List<CategoryDto>> GetListAsync(bool includeDisabled = true)
    {
        var query = dbContext.Categories
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (!includeDisabled)
        {
            query = query.Where(t => t.CategoryStatus == CategoryStatus.Normal);
        }

        return await query
            .OrderBy(t => t.ParentId)
            .ThenBy(t => t.Id)
            .Select(t => new CategoryDto
            {
                Id = t.Id,
                ParentId = t.ParentId,
                CategoryName = t.CategoryName,
                CreateTime = t.CreateTime,
                CategoryStatus = t.CategoryStatus,
                IsDeleted = t.IsDeleted
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<long>> CreateAsync(CategoryCreateDto dto)
    {
        var name = dto.CategoryName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<long>.Fail("分类名称不能为空");
        }

        // ParentId 为 0 或 null 即根分类，不再要求父级存在
        var parentId = dto.ParentId.GetValueOrDefault();
        if (parentId > 0 && !await ParentExistsAsync(parentId))
        {
            return ServiceResult<long>.Fail("指定的父级分类不存在");
        }

        var duplicated = await dbContext.Categories.AnyAsync(
            t => !t.IsDeleted && t.ParentId == parentId && t.CategoryName == name);
        if (duplicated)
        {
            return ServiceResult<long>.Conflict("同级下已存在同名分类");
        }

        var category = new Category
        {
            CategoryName = name,
            ParentId = parentId,
            // 枚举默认值是 Forbidden，必须显式赋值，否则新建分类全部是禁用状态
            CategoryStatus = dto.CategoryStatus ?? CategoryStatus.Normal,
            CreateTime = DateTime.UtcNow,
            IsDeleted = false
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        return ServiceResult<long>.Ok(category.Id);
    }

    public async Task<ServiceResult> UpdateAsync(long id, CategoryUpdateDto dto)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (category is null)
        {
            return ServiceResult.NotFound("分类不存在");
        }

        var name = dto.CategoryName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Fail("分类名称不能为空");
        }

        var parentId = dto.ParentId.GetValueOrDefault();
        if (parentId == id)
        {
            return ServiceResult.Fail("父级分类不能是自己");
        }

        if (parentId > 0)
        {
            if (!await ParentExistsAsync(parentId))
            {
                return ServiceResult.Fail("指定的父级分类不存在");
            }

            if (await WouldCreateCycleAsync(parentId, id))
            {
                return ServiceResult.Fail("上级分类不能选择自己的下级");
            }
        }

        var duplicated = await dbContext.Categories.AnyAsync(
            t => !t.IsDeleted && t.Id != id && t.ParentId == parentId && t.CategoryName == name);
        if (duplicated)
        {
            return ServiceResult.Conflict("同级下已存在同名分类");
        }

        category.CategoryName = name;
        category.ParentId = parentId;
        if (dto.CategoryStatus.HasValue)
        {
            category.CategoryStatus = dto.CategoryStatus.Value;
        }

        await dbContext.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(long id)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (category is null)
        {
            return ServiceResult.NotFound("分类不存在");
        }

        var hasChildren = await dbContext.Categories
            .AnyAsync(t => t.ParentId == id && !t.IsDeleted);
        if (hasChildren)
        {
            return ServiceResult.Fail("该分类下存在子分类，请先删除或转移子分类");
        }

        var hasDocuments = await dbContext.Documents.AnyAsync(d => d.CategoryId == id);
        if (hasDocuments)
        {
            return ServiceResult.Fail("该分类下存在关联的文章，无法直接删除");
        }

        // 软删除：实体本来就有 IsDeleted，物理删除会让审计与历史引用断链
        category.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private Task<bool> ParentExistsAsync(long parentId)
        => dbContext.Categories.AnyAsync(t => t.Id == parentId && !t.IsDeleted);

    private async Task<bool> WouldCreateCycleAsync(long candidateParentId, long selfId)
    {
        var parentOf = await dbContext.Categories
            .AsNoTracking()
            .Select(t => new { t.Id, t.ParentId })
            .ToDictionaryAsync(t => t.Id, t => t.ParentId);

        return HierarchyGuard.WouldCreateCycle(parentOf, candidateParentId, selfId);
    }
}
