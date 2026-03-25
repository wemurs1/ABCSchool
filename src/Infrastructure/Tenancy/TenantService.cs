using ABCShared.Library.Models.Requests.Tenancy;
using ABCShared.Library.Models.Responses.Tenancy;
using Application.Exceptions;
using Application.Features.Tenancy;
using AutoMapper;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy;

public class TenantService(
    IMultiTenantStore<ABCSchoolTenantInfo> tenantStore,
    ApplicationDbSeeder dbSeeder,
    IServiceProvider serviceProvider,
    IMapper mapper
) : ITenantService
{
    private readonly IMultiTenantStore<ABCSchoolTenantInfo> _tenantStore = tenantStore;
    private readonly ApplicationDbSeeder _dbSeeder = dbSeeder;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<string> ActivateAsync(string id, CancellationToken ct = default)
    {
        var tenantInDb = await _tenantStore.GetAsync(id) ?? throw new NotFoundException(["Tenant not found"]);
        tenantInDb.IsActive = true;
        var result = await _tenantStore.UpdateAsync(tenantInDb);
        if (!result) throw new Exception("Failed to save Tenant to DB");
        return tenantInDb.Identifier;
    }

    public async Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct = default)
    {
        var newTenant = new ABCSchoolTenantInfo
        {
            Id = createTenant.Identifier,
            Identifier = createTenant.Identifier,
            Name = createTenant.Name,
            IsActive = true,
            ConnectionString = createTenant.ConnectionString,
            Email = createTenant.Email,
            FirstName = createTenant.FirstName,
            LastName = createTenant.LastName,
            ValidUpTo = createTenant.ValidUpTo
        };

        var result = await _tenantStore.AddAsync(newTenant);
        if (!result) throw new Exception("Failed to save Tenant to DB");

        // Seeding tenant data
        using var scope = _serviceProvider.CreateScope();
        _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<ABCSchoolTenantInfo>(newTenant);
        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitialiseDatabaseAsync(ct);

        return newTenant.Identifier;
    }

    public async Task<string> DeactivateTenant(string id, CancellationToken ct = default)
    {
        var tenantInDb = await _tenantStore.GetAsync(id) ?? throw new NotFoundException(["Tenant not found"]);
        tenantInDb.IsActive = false;
        var result = await _tenantStore.UpdateAsync(tenantInDb);
        if (!result) throw new Exception("Failed to save Tenant to DB");
        return tenantInDb.Identifier;
    }

    public async Task<TenantResponse?> GetTenantByIdAsync(string id, CancellationToken ct = default)
    {
        var tenantInDb = await _tenantStore.GetAsync(id) ?? throw new NotFoundException(["Tenant not found"]);
        return _mapper.Map<TenantResponse>(tenantInDb);
    }

    public async Task<List<TenantResponse>> GetTenantsAsync(CancellationToken ct = default)
    {
        var tenantsInDb = await _tenantStore.GetAllAsync();
        return _mapper.Map<List<TenantResponse>>(tenantsInDb);
    }

    public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscriptionRequest, CancellationToken ct = default)
    {
        var tenantInDb = await _tenantStore.GetAsync(updateTenantSubscriptionRequest.TenantId) ?? throw new NotFoundException(["Tenant not found"]);
        tenantInDb.ValidUpTo = updateTenantSubscriptionRequest.NewExpiryDate;
        var result = await _tenantStore.UpdateAsync(tenantInDb);
        if (!result) throw new Exception("Failed to save Tenant to DB");
        return tenantInDb.Identifier;
    }
}
