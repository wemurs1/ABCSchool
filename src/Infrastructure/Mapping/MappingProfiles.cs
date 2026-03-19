using Application.Features.Identity.Roles;
using Application.Features.Schools;
using Application.Features.Tenancy;
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
    }
}
