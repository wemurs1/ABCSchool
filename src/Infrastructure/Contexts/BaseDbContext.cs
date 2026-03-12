using System.Reflection;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Identity.EntityFrameworkCore;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public abstract class BaseDbContext(IMultiTenantContextAccessor<ABCSchoolTenantInfo> multiTenantContextAccessor, DbContextOptions options)
    : MultiTenantIdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        string,
        IdentityUserClaim<string>,
        IdentityUserRole<string>,
        IdentityUserLogin<string>,
        ApplicationRoleClaim,
        IdentityUserToken<string>,
        IdentityUserPasskey<string>
    >(multiTenantContextAccessor, options)
{
    private new ABCSchoolTenantInfo? TenantInfo { get; set; } = multiTenantContextAccessor.MultiTenantContext.TenantInfo;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!string.IsNullOrEmpty(TenantInfo?.ConnectionString))
        {
            optionsBuilder.UseSqlServer(TenantInfo.ConnectionString, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
