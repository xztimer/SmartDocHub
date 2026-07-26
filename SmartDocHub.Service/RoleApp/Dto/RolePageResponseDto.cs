using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.RoleApp.Dto;

public class RolePageResponseDto:PageResponseDto
{
    public List<RoleDto> Roles { get; set; }
}
