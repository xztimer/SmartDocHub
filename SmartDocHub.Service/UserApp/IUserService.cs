using SmartDocHub.Service.Common;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public interface IUserService
{
    Task<UserPageResponseDto> GetPagedListAsync(UserPageRequestDto request);
    Task<UserDto?> GetAsync(long id);
    Task<bool> UpdateAsync(UserUpdateDto dto);
    Task<bool> DeleteAsync(long id);
    Task<bool> AssignRolesAsync(long userId, List<string> roleNames);
    List<UserAllDto> GetAll();
}
