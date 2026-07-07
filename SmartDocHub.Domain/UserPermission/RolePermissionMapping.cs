namespace SmartDocHub.Domain.UserPermission;

public class RolePermissionMapping : BaseEntity
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
}
