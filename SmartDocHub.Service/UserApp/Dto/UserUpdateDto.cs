using SmartDocHub.Domain.UserPermission;

using System.ComponentModel.DataAnnotations;

namespace SmartDocHub.Service.UserApp.Dto;

public class UserUpdateDto
{
    [MaxLength(20)]
    public string NickName { get; set; }
    public UserStatus Status { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
    public string? Password { get; set; }

    public List<string> RoleNames { get; set; }

}
