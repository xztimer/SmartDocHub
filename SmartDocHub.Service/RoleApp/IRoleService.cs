using SmartDocHub.Service.RoleApp.Dto;

namespace SmartDocHub.Service.RoleApp;

public interface IRoleService
{
    Task<List<RoleDto>> GetAll();
    Task<RolePageResponseDto> Query(RolePageRequestDto rolePageRequestDto);
}
