using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.RoleApp.Dto;

public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public RoleStatus Status { get; set; }

    public string? Remark { get; set; }
}
