using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Infrastructure.Contexts;
using Infrastructure.Tenancy;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Identity.Auth;

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
                .UseSqlServer(config.GetConnectionString("DefaultConnection")))
            .AddTransient<ITenantDbSeeder, TenantDbSeeder>()
            .AddTransient<ApplicationDbSeeder>()
            .AddIdentityService()
            .AddPermissions();
    }

    public static async Task AddDatabaseInitialiserAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>().InitialiseDatabaseAsync(cancellationToken);
    }

    internal static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        return services
            .AddIdentity<ApplicationUser, ApplicationRole>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services;
    }

    internal static IServiceCollection AddPermissions(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    public static IApplicationBuilder UseInfrastructureServices(this IApplicationBuilder app)
    {
        return app
            .UseMultiTenant();
    }
}
