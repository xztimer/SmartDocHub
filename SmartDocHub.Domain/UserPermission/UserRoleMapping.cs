namespace SmartDocHub.Domain.UserPermission;

public class UserRoleMapping
{
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long UserId { get; set; }

    public bool IsDeleted { get; set; } = false;
}
