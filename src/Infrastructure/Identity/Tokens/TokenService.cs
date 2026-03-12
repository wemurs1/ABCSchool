using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Exceptions;
using Application.Features.Identity.Tokens;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Tokens;

public class TokenService(
    UserManager<ApplicationUser> userManager,
    IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContextAccessor,
    RoleManager<ApplicationRole> roleManager)
: ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantContextAccessor = tenantContextAccessor;

    public async Task<TokenResponse> LoginAsync(TokenRequest request)
    {
        #region  Validation
        if (_tenantContextAccessor.MultiTenantContext.TenantInfo!.IsActive)
        {
            throw new UnauthorizedException(["Tenant subscription is not active. Contact your Administrator"]);
        }

        var userInDb = await _userManager.FindByNameAsync(request.Username) ?? throw new UnauthorizedException(["Authentication not successful"]);

        if (!await _userManager.CheckPasswordAsync(userInDb, request.Password)) throw new UnauthorizedException(["Incorrect Username or Password"]);

        if (!userInDb.IsActive) throw new UnauthorizedException(["User not active. Contact your Administrator"]);

        if (_tenantContextAccessor.MultiTenantContext.TenantInfo.Id is not TenancyConstants.Root.Id)
        {
            if (_tenantContextAccessor.MultiTenantContext.TenantInfo.ValidUpTo < DateTime.UtcNow)
            {
                throw new UnauthorizedException(["Subscription has expired. COntact your Administrator"]);
            }
        }
        #endregion Validation

        // Generate jwt
        return await GenerateTokenAndUpdateUserAsync(userInDb);

    }

    public Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        throw new NotImplementedException();
    }

    private async Task<TokenResponse> GenerateTokenAndUpdateUserAsync(ApplicationUser user)
    {
        // Generate jwt
        var newJwt = await GenerateToken(user);

        // Refresh token
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        await _userManager.UpdateAsync(user);

        return new TokenResponse
        {
            Jwt = newJwt,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiryDate = user.RefreshTokenExpiryTime
        };
    }

    private async Task<string> GenerateToken(ApplicationUser user)
    {
        // Generate encrypted token
        return GenerateEncryptedToken(GenerateSigningCredentials(), await GetUserClaims(user));
    }

    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private SigningCredentials GenerateSigningCredentials()
    {
        byte[] secret = Encoding.UTF8.GetBytes("sdfggdfhdgflksdfklgsdlkgdslkngsdklgsd");
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }

    private async Task<IEnumerable<Claim>> GetUserClaims(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        var roleClaims = new List<Claim>();
        var permissionClaims = new List<Claim>();

        foreach (var userRole in userRoles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, userRole));
            var currentRole = await _roleManager.FindByNameAsync(userRole) ?? throw new Exception("Role not found");
            var allPermissionsForCurrentRole = await _roleManager.GetClaimsAsync(currentRole);

            permissionClaims.AddRange(allPermissionsForCurrentRole);
        }

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Email,user.Email??string.Empty),
            new (ClaimTypes.Name,user.FirstName),
            new (ClaimTypes.Surname, user.LastName),
            new (ClaimConstants.Tenant, _tenantContextAccessor.MultiTenantContext.TenantInfo!.Id),
            new (ClaimTypes.MobilePhone, user.PhoneNumber??string.Empty)
        }
        .Union(roleClaims)
        .Union(userClaims)
        .Union(permissionClaims);

        return claims;
    }

    private string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
