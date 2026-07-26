using AutoMapper;

using Microsoft.EntityFrameworkCore;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.DepartmentApp.Dto;

namespace SmartDocHub.Service.DepartmentApp;

public class DepartmentService(SmartDocHubDbContext dbContext, IMapper mapper) : IDepartmentService
{
    //public async Task<List<Department>> GetTreeAsync()
    //{
    //    var allDepts = await dbContext.Set<Department>()
    //        .OrderBy(d => d.Sort)
    //        .ToListAsync();

    //    var dtos = mapper.Map<List<DepartmentDto>>(allDepts);
    //    return BuildTree(dtos);
    //}

    public async Task<Department> GetAsync(long id)
    {
        var entity = await dbContext.Departments.FirstOrDefaultAsync(d => d.Id == id);
        return entity;
    }

    //public async Task<Department> AddAsync(Department department)
    //{
    //    if(department.ParentId == 0)
    //    {
    //        throw new Exception("请选择上级部门");
    //    }
    //    return 
    //}

    public async Task<bool> UpdateAsync(DepartmentUpdateDto dto)
    {
        var entity = await dbContext.Set<Department>().FirstOrDefaultAsync(d => d.Id == dto.Id);
        if (entity == null)
            return false;

        mapper.Map(dto, entity);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var hasChildren = await dbContext.Set<Department>().AnyAsync(d => d.ParentId == id);
        if (hasChildren)
            return false;

        var entity = await dbContext.Set<Department>().FirstOrDefaultAsync(d => d.Id == id);
        if (entity == null)
            return false;

        dbContext.Set<Department>().Remove(entity);
        await dbContext.SaveChangesAsync();
        return true;
    }

    //private List<DepartmentDto> BuildTree(List<DepartmentDto> allDepts)
    //{
    //    var lookup = allDepts.ToDictionary(d => d.Id);
    //    var roots = new List<DepartmentDto>();

    //    foreach (var dept in allDepts)
    //    {
    //        if (lookup.TryGetValue(dept.ParentId, out var parent))
    //        {
    //            parent.Children.Add(dept);
    //        }
    //        else
    //        {
    //            roots.Add(dept);
    //        }
    //    }

    //    return roots;
    //}
}
