using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Infrastructure.Tenancy;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class ApplicationDbSeeder(
    IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext applicationDbContext
)
{
    private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantInfoContextAccessor = tenantContextAccessor;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task InitialiseDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_applicationDbContext.Database.GetMigrations().Any())
        {
            if ((await _applicationDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await _applicationDbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _applicationDbContext.Database.CanConnectAsync(cancellationToken))
            {
                // Seeding
                // Default Roles > Assign Permissions/Claims
                await InitialiseDefaultRolesAsync(cancellationToken);

                // Users > Assign Roles
                await InitialiseAdminUserAsync(cancellationToken);
            }
        }
    }

    private async Task InitialiseDefaultRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in RoleConstants.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken) is not ApplicationRole incomingRole)
            {
                incomingRole = new ApplicationRole()
                {
                    Name = roleName,
                    Description = $"{roleName} Role"
                };

                await _roleManager.CreateAsync(incomingRole);
            }

            // Assign permissions
            if (roleName == RoleConstants.Basic)
            {
                // Assign basic permissions
                await AssignPermissionsToRoleAsync(SchoolPermissions.Basic, incomingRole, cancellationToken);
            }
            else if (roleName == RoleConstants.Admin)
            {
                // Assign admin permissions
                await AssignPermissionsToRoleAsync(SchoolPermissions.Admin, incomingRole, cancellationToken);

                if (_tenantInfoContextAccessor.MultiTenantContext.TenantInfo?.Id == TenancyConstants.Root.Id)
                {
                    await AssignPermissionsToRoleAsync(SchoolPermissions.Root, incomingRole, cancellationToken);
                }
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(
        IReadOnlyList<SchoolPermission> rolePermissions,
        ApplicationRole role,
        CancellationToken cancellationToken
    )
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var rolePermission in rolePermissions)
        {
            if (!currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == rolePermission.Name))
            {
                _applicationDbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = ClaimConstants.Permission,
                    ClaimValue = rolePermission.Name,
                    Description = rolePermission.Description,
                    Group = rolePermission.Group
                });
            }
        }
        await _applicationDbContext.SaveChangesAsync();
    }

    private async Task InitialiseAdminUserAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_tenantInfoContextAccessor.MultiTenantContext.TenantInfo?.Email)) return;

        if (await _userManager.Users.SingleOrDefaultAsync(
            user => user.Email == _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email, cancellationToken) is not ApplicationUser incomingUser)
        {
            incomingUser = new ApplicationUser()
            {
                FirstName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.FirstName,
                LastName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.LastName,
                Email = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                UserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                NormalizedUserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                IsActive = true
            };

            var passwordHash = new PasswordHasher<ApplicationUser>();
            incomingUser.PasswordHash = passwordHash.HashPassword(incomingUser, TenancyConstants.DefaultPassword);
            await _userManager.CreateAsync(incomingUser);
        }

        if (!await _userManager.IsInRoleAsync(incomingUser, RoleConstants.Admin))
        {
            await _userManager.AddToRoleAsync(incomingUser, RoleConstants.Admin);
        }
    }
}
