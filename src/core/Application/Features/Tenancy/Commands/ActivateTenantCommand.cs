using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class ActivateTenantCommand : IRequest<IResponseWrapper>
{
    public required string TenantId { get; set; }
}

public class ActivateTenantCommandHandler(ITenantService tenantService) : IRequestHandler<ActivateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantService.ActivateAsync(request.TenantId, cancellationToken);
        return ResponseWrapper<string>.Success(data: tenantId, message: "Tenant activated successfully");
    }
}