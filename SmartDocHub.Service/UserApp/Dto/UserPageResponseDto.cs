using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.UserApp.Dto;

public class UserPageResponseDto:PageResponseDto
{
    public List<UserDto> Users { get; set; }
}
