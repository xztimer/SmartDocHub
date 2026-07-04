using AutoMapper;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.UserApp.Dto;

namespace SmartDocHub.Service;

public class DocHubProfile : Profile
{
    public DocHubProfile()
    {
        CreateMap<User, UserCreateDto>().ReverseMap();
    }
}
