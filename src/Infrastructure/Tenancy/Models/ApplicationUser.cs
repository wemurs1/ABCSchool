using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Tenancy.Models;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
