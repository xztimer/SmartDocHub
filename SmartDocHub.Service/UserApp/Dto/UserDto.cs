using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.RoleApp.Dto;

namespace SmartDocHub.Service.UserApp.Dto;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public long DeptId { get; set; }
    public string DeptName { get; set; }
    public string NickName { get; set; }
    public UserStatus Status { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
    public string? Remark { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public  DateTime? DisabledTime { get; set; }
}
