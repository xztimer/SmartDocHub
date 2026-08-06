using SmartDocHub.Service.Common;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public interface IUserService
{
    Task<UserPageResponseDto> GetPagedListAsync(UserPageRequestDto request);
    List<UserAllDto> GetAll();
}
