using ABCShared.Library.Constants;
using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Models.Responses.Identity;
using Application.Exceptions;
using Application.Features.Identity.Users;
using AutoMapper;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext context,
    IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor,
    IMapper mapper) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantContextAccessor = tenantContextAccessor;

    public async Task<string> ActivateOrDeactivateAsync(string userId, bool activation, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(userId);
        userInDb.IsActive = activation;
        var result = await _userManager.UpdateAsync(userInDb);
        if (!result.Succeeded) throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        return userId;
    }

    public async Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(userId);
        if (await _userManager.IsInRoleAsync(userInDb, RoleConstants.Admin) && request.UserRoles.Any(ur => !ur.IsAssigned && ur.Name == RoleConstants.Admin))
        {
            var adminUsersCount = (await _userManager.GetUsersInRoleAsync(RoleConstants.Admin)).Count;
            if (userInDb.Email == TenancyConstants.Root.Email)
            {
                if (_tenantContextAccessor.MultiTenantContext.TenantInfo?.Id == TenancyConstants.Root.Id)
                {
                    throw new ConflictException(["Not allowed to remove Admin role from a Root tenant user"]);
                }
            }
            else if (adminUsersCount <= 2)
            {
                throw new ConflictException(["Not allowed. Tenant should have at least two Admin users"]);
            }
        }

        foreach (var userRole in request.UserRoles)
        {
            if (userRole.IsAssigned)
            {
                if (!await _userManager.IsInRoleAsync(userInDb, userRole.Name))
                {
                    await _userManager.AddToRoleAsync(userInDb, userRole.Name);
                }
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(userInDb, userRole.Name);
            }
        }

        return userId;
    }

    public async Task<string> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(request.UserId);
        if (request.NewPassword != request.ConfirmNewPassword) throw new ConflictException(["Passwords do not match"]);

        var result = await _userManager.ChangePasswordAsync(userInDb, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        return userInDb.Id;
    }

    public async Task<string> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword) throw new ConflictException(["Passwords do not match"]);
        if (await IsEmailTakenAsync(request.Email)) throw new ConflictException(["Email address is taken"]);
        var newUser = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive,
            UserName = request.Email,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(newUser, request.Password);
        if (!result.Succeeded) throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        return newUser.Id;
    }

    public async Task<string> DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(userId);
        if (userInDb.Email == TenancyConstants.Root.Email) throw new ConflictException(["Not allowed to remove an Admin user for a Root tenant"]);
        var result = await _userManager.DeleteAsync(userInDb);
        if (!result.Succeeded) throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        return userId;
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var usersInDb = await _userManager.Users.ToListAsync(cancellationToken);
        return _mapper.Map<List<UserResponse>>(usersInDb);
    }

    public async Task<UserResponse> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(userId);
        return _mapper.Map<UserResponse>(userInDb);
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(userId);
        var userRoleNames = await _userManager.GetRolesAsync(userInDb);
        var permissions = new List<string>();
        foreach (var role in await _roleManager.Roles.Where(r => userRoleNames.Contains(r.Name!)).ToListAsync(cancellationToken))
        {
            permissions.AddRange(
                await _context.RoleClaims
                    .Where(rc => rc.RoleId == role.Id && rc.ClaimType == ClaimConstants.Permission)
                    .Select(rc => rc.ClaimValue!)
                    .ToListAsync()
            );
        }
        return permissions.Distinct().ToList();
    }

    public async Task<List<UserRoleResponse>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(userId);
        var userRoles = new List<UserRoleResponse>();
        var rolesInDb = await _roleManager.Roles.ToListAsync(cancellationToken);
        foreach (var role in rolesInDb)
        {
            userRoles.Add(new UserRoleResponse
            {
                RoleId = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsAssigned = await _userManager.IsInRoleAsync(userInDb, role.Name!)
            });
        }
        return userRoles;
    }

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<bool> IsPermissionAssigned(string userId, string permission, CancellationToken cancellationToken = default)
    {
        return (await GetUserPermissionsAsync(userId, cancellationToken)).Contains(permission);
    }

    public async Task<string> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userInDb = await GetUserAsync(request.Id);
        userInDb.FirstName = request.FirstName;
        userInDb.LastName = request.LastName;
        userInDb.PhoneNumber = request.PhoneNumber;
        var result = await _userManager.UpdateAsync(userInDb);
        if (!result.Succeeded) throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(result));
        return userInDb.Id;
    }

    private async Task<ApplicationUser> GetUserAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException(["User does not exist"]);
    }
}
