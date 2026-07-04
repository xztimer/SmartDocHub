namespace SmartDocHub.Domain.UserPermission;

public class RolePermissionMapping
{
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public bool IsDeleted { get; set; } = false;
}
