using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.PermissionApp.Dto;

public class PermissionDto
{
    public long? ParentId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public PermissionType Type { get; set; }
    public string? Path { get; set; }
    public string? ApiMethod { get; set; }
    public string? Icon { get; set; }
    public PermissionStatus Status { get; set; }
}
