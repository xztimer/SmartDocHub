using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.UserApp.Dto;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string NickName { get; set; }
    public UserStatus Status { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public DateTime CreateTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public  DateTime? DisabledTime { get; set; }
}
