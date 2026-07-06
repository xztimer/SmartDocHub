using Microsoft.AspNetCore.Identity;

namespace SmartDocHub.Domain.UserPermission;

public class User : IdentityUser<long>
{
    public string NickName { get; set; }
    public long? DeptId { get; set; }

    public string? Remark { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginTime { get; set; }

    public DateTime? UpdateTime { get; set; }
}

public enum UserStatus
{
    Forbidden = 0,
    Normal = 1
}

public enum Gender
{
    Male = 1,
    Female = 2
}
