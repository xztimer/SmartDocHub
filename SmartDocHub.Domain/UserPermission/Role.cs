using Microsoft.AspNetCore.Identity;

namespace SmartDocHub.Domain.UserPermission;

public class Role:IdentityRole<long>
{
    public string? Remark { get; set; }

    public RoleStatus Status { get; set; }
}


public enum RoleStatus
{
    Forbidden = 0,
    Normal = 1
}
