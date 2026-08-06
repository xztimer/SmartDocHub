using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.DepartmentApp.Dto;

public class DepartmentCreateDto
{
    public long? ParentId { get; set; }
    public string DeptName { get; set; }
    public string? Code { get; set; }
    public int Sort { get; set; }
    public string Description { get; set; } = string.Empty;
    public DepartmentStatus Status { get; set; }
}
