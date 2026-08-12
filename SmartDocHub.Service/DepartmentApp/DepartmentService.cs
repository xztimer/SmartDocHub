using AutoMapper;

using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.DepartmentApp.Dto;

namespace SmartDocHub.Service.DepartmentApp;

public class DepartmentService(SmartDocHubDbContext dbContext, IMapper mapper) : IDepartmentService
{
    public async Task<Department?> GetAsync(long id)
    {
        return await dbContext.Departments.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
    }

    public async Task<List<DepartmentDto>> GetListAsync(string? key)
    {
        var departments = await dbContext.Departments.Where(t=>!t.IsDeleted)
            .ToListAsync();
        var dtos = mapper.Map<List<DepartmentDto>>(departments);

        if (string.IsNullOrWhiteSpace(key))
        {
            return dtos.OrderBy(t => t.ParentId).ThenBy(t => t.Sort).ToList(); ;
        }

        var dtoDict = dtos.ToDictionary(d => d.Id);
        var resultSet = new HashSet<DepartmentDto>();
        var matchedNodes = dtos.Where(d => d.DeptName.Contains(key));

        foreach (var node in matchedNodes)
        {
            var current = node;
            while (current != null)
            {
                if (!resultSet.Add(current))
                {
                    break;
                }
                if (current.ParentId.HasValue && dtoDict.TryGetValue(current.ParentId.Value, out var parent))
                {
                    current = parent;
                }
                else
                {
                    current = null;
                }
            }
        }

        return resultSet.OrderBy(t => t.ParentId).ThenBy(t => t.Sort).ToList();
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await dbContext.Departments
            .Where(t => t.Status == DepartmentStatus.Normal && !t.IsDeleted)
            .ToListAsync();
        var dtos = mapper.Map<List<DepartmentDto>>(departments);

        //var childGroup = dtos.GroupBy(t => t.ParentId)
        //    .Where(g => g.Key.HasValue)
        //    .ToDictionary(t => t.Key, t => t.OrderBy(t => t.Sort).ToList());
        //var result = new List<DepartmentDto>();
        //var rootNodes = dtos
        //    .Where(t => t.ParentId == 0 || t.ParentId==null)
        //    .OrderBy(t => t.Sort)
        //    .ToList();
        //foreach (var ndoe in rootNodes)
        //{
        //    BuildTreeOrder(ndoe, childGroup, result);
        //}

        //return result;a
        return dtos.OrderBy(t => t.ParentId).ThenBy(t => t.Sort).ToList();

    }

    private void BuildTreeOrder(
        DepartmentDto currentNode,
        Dictionary<long?, List<DepartmentDto>> childrenGroup,
        List<DepartmentDto> result)
    {
        result.Add(currentNode);

        if (childrenGroup.TryGetValue(currentNode.Id, out var children))
        {
            foreach (var child in children)
            {
                BuildTreeOrder(child, childrenGroup, result);
            }
        }
    }

    public async Task<(bool IsSuccess, string Message, Department? Entity)> CreateAsync(DepartmentCreateDto dto)
    {
        if (dto.ParentId == 0) dto.ParentId = null;

        var hasConflict = await dbContext.Departments.AnyAsync(t =>
            !t.IsDeleted &&
            t.ParentId == dto.ParentId &&
            (t.DeptName == dto.DeptName || (!string.IsNullOrEmpty(dto.Code) && t.Code == dto.Code)));

        if (hasConflict)
        {
            return (false, "同级下已存在相同的部门名称或部门编码", null);
        }

        var entity = mapper.Map<Department>(dto);

        if (entity.Sort == 0)
        {
            var maxSort = await dbContext.Departments
                .Where(t => !t.IsDeleted && t.ParentId == entity.ParentId)
                .Select(t => (int?)t.Sort)
                .MaxAsync();
            entity.Sort = (maxSort ?? 0) + 1;
        }

        await dbContext.Departments.AddAsync(entity);
        await dbContext.SaveChangesAsync();

        return (true, string.Empty, entity);
    }

    public async Task<(bool IsSuccess, string Message)> UpdateAsync(DepartmentUpdateDto dto)
    {
        var entity = await dbContext.Departments
            .Include(d => d.Children)
            .FirstOrDefaultAsync(d => d.Id == dto.Id && !d.IsDeleted);

        if (entity == null) return (false, "部门不存在");

        if (dto.ParentId == dto.Id)
        {
            return (false, "上级部门不能选自己");
        }

        var hasConflict = await dbContext.Departments.AnyAsync(t =>
            !t.IsDeleted &&
            t.Id != dto.Id &&
            t.ParentId == dto.ParentId &&
            (t.DeptName == dto.DeptName || (!string.IsNullOrEmpty(dto.Code) && t.Code == dto.Code)));

        if (hasConflict)
        {
            return (false, "同级下已存在相同的部门名称或部门编码");
        }

        mapper.Map(dto, entity);
        await dbContext.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool IsSuccess, string Message)> DeleteAsync(long id)
    {
        var department = await dbContext.Departments
            .Include(d => d.Children.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (department == null)
        {
            return (false, "要删除的部门不存在");
        }
        if (department.Children.Any())
        {
            return (false, "该部门下仍存在子部门，请先删除或转移子部门");
        }

        var hasUser = await dbContext.Users.AnyAsync(t => t.DeptId == id);
        if (hasUser)
        {
            return (false, "该部门存在用户，不能删除");
        }

        department.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return (true, string.Empty);
    }
}