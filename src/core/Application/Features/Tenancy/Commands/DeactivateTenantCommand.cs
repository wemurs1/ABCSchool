using Application.Wrappers;
using MediatR;

namespace Application.Features.Tenancy.Commands;

public class DeactivateTenantCommand : IRequest<IResponseWrapper>
{
    public required string TenantId { get; set; }
}

public class DeactivateTenantCommandHandler(ITenantService tenantService) : IRequestHandler<DeactivateTenantCommand, IResponseWrapper>
{
    private readonly ITenantService _tenantService = tenantService;

    public async Task<IResponseWrapper> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantService.DeactivateTenant(request.TenantId, cancellationToken);
        return ResponseWrapper<string>.Success(data: tenantId, message: "Tenant deactivated successfully");
    }
}