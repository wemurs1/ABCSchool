using ABCShared.Library.Models.Responses.Tenancy;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Queries;

public class GetTenantsQuery : IRequest<IResponseWrapper> { }

public class GetTenantsQueryHandler(ITenantService tenantService) : IRequestHandler<GetTenantsQuery, IResponseWrapper>
{
    private ITenantService _tenantService = tenantService;
    public async Task<IResponseWrapper> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenantResponses = await _tenantService.GetTenantsAsync(cancellationToken);
        return ResponseWrapper<List<TenantResponse>>.Success(data: tenantResponses);
    }
}