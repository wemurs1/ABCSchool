using ABCShared.Library.Models.Responses.Tenancy;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Queries;

public class GetTenantByIdQuery : IRequest<IResponseWrapper>
{
    public required string TenantId { get; set; }
}

public class GetTenantByIdQueryHandler(ITenantService tenantService) : IRequestHandler<GetTenantByIdQuery, IResponseWrapper>
{
    public readonly ITenantService _tenantService = tenantService;
    public async Task<IResponseWrapper> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantResponse = await _tenantService.GetTenantByIdAsync(request.TenantId, cancellationToken);
        if (tenantResponse is not null)
        {
            return ResponseWrapper<TenantResponse>.Success(data: tenantResponse);
        }
        return ResponseWrapper<TenantResponse>.Fail(message: "Tenant does not exist");
    }
}