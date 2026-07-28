using AutoMapper;

using SmartDocHub.Domain.AuditLog;
using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.AuditLogApp.Dto;
using SmartDocHub.Service.RoleApp.Dto;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service;

public class DocHubProfile : Profile
{
    public DocHubProfile()
    {
        CreateMap<User, UserCreateDto>().ReverseMap();
        CreateMap<User, UserDto>();
        CreateMap<User, UserAllDto>();

        CreateMap<Role, RoleDto>().ReverseMap();
        CreateMap<RoleCreateDto, Role>();
        CreateMap<RoleUpdateDto, Role>();

        CreateMap<SysLog, SysLogDto>().ReverseMap();
       
    }
}
