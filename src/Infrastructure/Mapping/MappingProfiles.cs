using Application.Features.Tenancy;
using AutoMapper;
using Infrastructure.Tenancy;

namespace Application.Mapping;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ABCSchoolTenantInfo, TenantResponse>();
    }
}
