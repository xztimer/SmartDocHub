namespace SmartDocHub.Service.UserApp.Dto;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }
    public Gender Gender { get; set; }
    public long DeptId { get; set; }
    public string DeptName { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public List<long> RoleIds { get; set; } = new();
    public List<string> RoleNames { get; set; } = new();
}

public class UserUpdateDto
{
    public string NickName { get; set; }
    public string Email { get; set; }
    public Gender Gender { get; set; }
    public long DeptId { get; set; }
}

public class UserQueryDto
{
    public string UserName { get; set; }
    public string NickName { get; set; }
    public UserStatus? Status { get; set; }
    public long? DeptId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AssignUserRoleDto
{
    public long UserId { get; set; }
    public List<long> RoleIds { get; set; } = new();
}

public class ChangePasswordDto
{
    public long UserId { get; set; }
    public string NewPassword { get; set; }
}

public class ResetUserStatusDto
{
    public long UserId { get; set; }
    public UserStatus Status { get; set; }
}
