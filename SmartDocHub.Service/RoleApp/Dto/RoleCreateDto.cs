using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.RoleApp.Dto;

public class RoleCreateDto
{
    public string Name { get; set; }
    public string? Remark { get; set; }
    public List<long>? PermissionIds { get; set; }
}

public class RoleUpdateDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public RoleStatus Status { get; set; }
    public string? Remark { get; set; }
    public List<long>? PermissionIds { get; set; }
}
