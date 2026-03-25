using ABCShared.Library.Constants;
using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Models.Responses.Identity;
using Application.Exceptions;
using Application.Features.Identity.Roles;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext applicationDbContext,
    IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantInfo,
    IMapper mapper) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantInfo = tenantInfo;

    public async Task<string> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var newRole = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description
        };
        var result = await _roleManager.CreateAsync(newRole);
        if (!result.Succeeded)
        {
            throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        }
        return newRole.Name;
    }

    public async Task<string> DeleteAsync(string id, CancellationToken ct = default)
    {
        var roleInDB = await _roleManager.FindByIdAsync(id) ?? throw new NotFoundException(["Role does not exist"]);
        if (roleInDB.Name is null) throw new Exception("Role has no name");
        if (RoleConstants.IsDefaultRole(roleInDB.Name))
        {
            throw new ConflictException([$"You are not allowed to delete '{roleInDB.Name}' role"]);
        }

        if ((await _userManager.GetUsersInRoleAsync(roleInDB.Name)).Count > 0)
        {
            throw new ConflictException([$"You are not allowed to delete '{roleInDB.Name}' role as it is currently assigned to a user"]);
        }

        var result = await _roleManager.DeleteAsync(roleInDB);
        if (!result.Succeeded)
        {
            throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        }

        return roleInDB.Name;
    }

    public async Task<bool> DoesItExistsAsync(string name, CancellationToken ct = default)
    {
        return await _roleManager.RoleExistsAsync(name);
    }

    public async Task<List<RoleResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _roleManager.Roles.ProjectTo<RoleResponse>(_mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public async Task<RoleResponse> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var roleInDb = await _applicationDbContext.Roles.FindAsync(id, ct) ?? throw new NotFoundException(["Role does not exist"]);
        return _mapper.Map<RoleResponse>(roleInDb);
    }

    public async Task<RoleResponse> GetRoleWithPermissionsAsync(string id, CancellationToken ct = default)
    {
        var role = await GetByIdAsync(id, ct);

        role.Permissions = await _applicationDbContext.RoleClaims
            .Where(rc => rc.RoleId == role.Id && rc.ClaimType == ClaimConstants.Permission)
            .Select(rc => rc.ClaimValue)
            .ToListAsync(ct);

        return role;
    }

    public async Task<string> UpdateAsync(UpdateRoleRequest request, CancellationToken ct = default)
    {
        var roleInDb = await _roleManager.FindByIdAsync(request.Id) ?? throw new NotFoundException(["Role does not exist"]);
        if (roleInDb.Name is null) throw new Exception("Role has no name");
        if (RoleConstants.IsDefaultRole(roleInDb.Name))
        {
            throw new ConflictException([$"Changes not allowed on system role '{roleInDb.Name}'"]);
        }

        roleInDb.Name = request.Name;
        roleInDb.Description = request.Description;
        roleInDb.NormalizedName = request.Name.ToUpperInvariant();
        var result = await _roleManager.UpdateAsync(roleInDb);
        if (!result.Succeeded)
        {
            throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        }

        return roleInDb.Name;
    }

    public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken ct = default)
    {
        var roleInDb = await _roleManager.FindByIdAsync(request.RoleId) ?? throw new NotFoundException(["Role does not exist"]);
        if (roleInDb.Name == RoleConstants.Admin)
        {
            throw new ConflictException([$"Not allowed to change permissions for '{roleInDb.Name}' role"]);
        }

        if (_tenantInfo.MultiTenantContext.TenantInfo?.Id != TenancyConstants.Root.Id)
        {
            request.NewPermissions.RemoveAll(p => p.StartsWith("Permission.Tenants"));
        }

        var currentClaims = await _roleManager.GetClaimsAsync(roleInDb);
        foreach (var claim in currentClaims.Where(c => !request.NewPermissions.Any(p => p == c.Value)))
        {
            var result = await _roleManager.RemoveClaimAsync(roleInDb, claim);
            if (!result.Succeeded)
            {
                throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
            }
        }

        foreach (var newPermission in request.NewPermissions.Where(p => !currentClaims.Any(c => c.Value == p)))
        {
            _applicationDbContext.RoleClaims.Add(new ApplicationRoleClaim
            {
                RoleId = roleInDb.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = newPermission,
                Description = "",
                Group = ""
            });
        }
        var dbResult = await _applicationDbContext.SaveChangesAsync(ct) > 0;
        if (!dbResult) return "No permissions updated";

        return "Permissions updated successufully";
    }
}
