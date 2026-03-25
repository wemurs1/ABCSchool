using ABCShared.Library.Models.Requests.Schools;
using ABCShared.Library.Models.Responses.Identity;
using ABCShared.Library.Models.Responses.Schools;
using ABCShared.Library.Models.Responses.Tenancy;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;

namespace Application.Mapping;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ABCSchoolTenantInfo, TenantResponse>();
        CreateMap<School, SchoolResponse>();
        CreateMap<CreateSchoolRequest, School>();
        CreateMap<ApplicationRole, RoleResponse>();
        CreateMap<ApplicationUser, UserResponse>();
    }
}
