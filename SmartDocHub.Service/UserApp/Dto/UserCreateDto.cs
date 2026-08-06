using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.UserApp.Dto;

public class UserCreateDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string NickName { get; set; }
    public string? Remark { get; set; }
    public UserStatus Status { get; set; }
}
