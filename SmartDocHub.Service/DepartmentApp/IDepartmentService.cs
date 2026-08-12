using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.DepartmentApp.Dto;

namespace SmartDocHub.Service.DepartmentApp;

public interface IDepartmentService
{
    Task<(bool IsSuccess, string Message, Department? Entity)> CreateAsync(DepartmentCreateDto dto);
    Task<(bool IsSuccess, string Message)> DeleteAsync(long id);
    Task<List<DepartmentDto>> GetAllAsync();
    Task<Department?> GetAsync(long id);
    Task<List<DepartmentDto>> GetListAsync(string? key);
    Task<(bool IsSuccess, string Message)> UpdateAsync(DepartmentUpdateDto dto);
}
