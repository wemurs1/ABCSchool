using ABCShared.Library.Models.Requests.Tenancy;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class UpdateTenantSubscriptionCommand : IRequest<IResponseWrapper>
{
    public required UpdateTenantSubscriptionRequest UpdateTenantSubscription { get; set; }
}

public class UpdateTenantSubscriptionCommandHandler(ITenantService tenantService) : IRequestHandler<UpdateTenantSubscriptionCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(UpdateTenantSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantService.UpdateSubscriptionAsync(
            request.UpdateTenantSubscription,
            cancellationToken
        );
        return ResponseWrapper<string>.Success(data: tenantId, message: "Tenant subscription updated successfully");
    }
}