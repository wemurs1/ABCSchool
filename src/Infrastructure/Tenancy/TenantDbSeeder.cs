using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy;

public class TenantDbSeeder(TenantDbContext tenantDbContext, IServiceProvider serviceProvider) : ITenantDbSeeder
{
    private readonly TenantDbContext _tenantDbContext = tenantDbContext;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InitialiseDatabaseAsync(CancellationToken cancellationToken)
    {
        // Seed Tenant data
        await InitialiseDatabaseWithTenantAsync(cancellationToken);

        foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
        {
            // Application Db Seeder
            await InitialiseApplicationDbForTenantAsync(tenant, cancellationToken);
        }
    }

    private async Task InitialiseDatabaseWithTenantAsync(CancellationToken cancellationToken)
    {
        if (await _tenantDbContext.TenantInfo.FindAsync([TenancyConstants.Root.Id], cancellationToken) is null)
        {
            // Create tenant
            var rootTenant = new ABCSchoolTenantInfo
            {
                Id = TenancyConstants.Root.Id,
                Identifier = TenancyConstants.Root.Id,
                Name = TenancyConstants.Root.Name,
                Email = TenancyConstants.Root.Email,
                FirstName = TenancyConstants.FirstName,
                LastName = TenancyConstants.LastName,
                IsActive = true,
                ValidUpTo = DateTime.UtcNow.AddYears(2)
            };

            _tenantDbContext.TenantInfo.Add(rootTenant);
            await _tenantDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task InitialiseApplicationDbForTenantAsync(ABCSchoolTenantInfo currentTenant, CancellationToken cancellationToken)
    {
        _serviceProvider.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = new MultiTenantContext<ABCSchoolTenantInfo>(currentTenant);

        await _serviceProvider.GetRequiredService<ApplicationDbSeeder>().InitialiseDatabaseAsync(cancellationToken);
    }
}
