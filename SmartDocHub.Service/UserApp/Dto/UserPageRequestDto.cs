using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.UserApp.Dto;

public class UserPageRequestDto : PageRequestDto
{
    public string? UserName { get; set; }
    public long? DeptId { get; set; }
}
    