using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Infrastructure.Contexts;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddDbContext<TenantDbContext>(options => options
                .UseSqlServer(config.GetConnectionString("DefaultConnection")))
            .AddMultiTenant<ABCSchoolTenantInfo>()
                .WithHeaderStrategy(TenancyConstants.TenantIdName)
                .WithClaimStrategy(TenancyConstants.TenantIdName)
                .WithEFCoreStore<TenantDbContext, ABCSchoolTenantInfo>()
                .Services
            .AddDbContext<ApplicationDbContext>(options => options
                .UseSqlServer(config.GetConnectionString("DefaultConnection")));
    }

    public static IApplicationBuilder UseInfrastructureServices(this IApplicationBuilder app)
    {
        return app
            .UseMultiTenant();
    }
}
