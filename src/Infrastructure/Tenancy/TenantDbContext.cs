using Finbuckle.MultiTenant.EntityFrameworkCore.Stores;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenancy;

public class TenantDbContext(DbContextOptions<TenantDbContext> options) : EFCoreStoreDbContext<ABCSchoolTenantInfo>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ABCSchoolTenantInfo>().ToTable("Tenants", "Multitenancy");
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant=>tenant.Id).IsRequired().HasMaxLength(64);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant=>tenant.Identifier).IsRequired().HasMaxLength(450);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant => tenant.Name).IsRequired().HasMaxLength(60);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant => tenant.Email).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant => tenant.FirstName).IsRequired().HasMaxLength(60);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant => tenant.LastName).IsRequired().HasMaxLength(60);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant=>tenant.ConnectionString).IsRequired(false).HasMaxLength(450);
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant=>tenant.ValidUpTo).IsRequired();
        modelBuilder.Entity<ABCSchoolTenantInfo>().Property(tenant=>tenant.IsActive).IsRequired();
    }
}
