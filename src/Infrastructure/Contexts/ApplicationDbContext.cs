using Domain.Entities;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class ApplicationDbContext(IMultiTenantContextAccessor<ABCSchoolTenantInfo> multiTenantContextAccessor, DbContextOptions<ApplicationDbContext> options)
: BaseDbContext(multiTenantContextAccessor, options)
{
    public DbSet<School> Schools { get; set; }
}
