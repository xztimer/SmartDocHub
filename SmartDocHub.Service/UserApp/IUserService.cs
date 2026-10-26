using SmartDocHub.Service.Common;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service.UserApp;

public interface IUserService
{
    Task<UserPageResponseDto> GetPagedListAsync(UserPageRequestDto request);
    Task<List<UserAllDto>> GetAllAsync();

    /// <summary>
    /// 更新用户资料、口令与角色。三步在同一事务内完成，任一失败整体回滚。
    /// </summary>
    Task<ServiceResult> UpdateAsync(long id, UserUpdateDto dto);
}
