using ABCShared.Library.Models.Requests.Tenancy;
using ABCShared.Library.Models.Responses.Tenancy;

namespace Application.Features.Tenancy;

public interface ITenantService
{
    Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct = default);
    Task<string> ActivateAsync(string id, CancellationToken ct = default);
    Task<string> DeactivateTenant(string id, CancellationToken ct = default);
    Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest, CancellationToken ct = default);
    Task<List<TenantResponse>> GetTenantsAsync(CancellationToken ct = default);
    Task<TenantResponse?> GetTenantByIdAsync(string id, CancellationToken ct = default);
}
