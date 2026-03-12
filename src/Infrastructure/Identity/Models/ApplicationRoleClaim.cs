using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Models;

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public required string Description { get; set; }
    public required string Group { get; set; }
}
