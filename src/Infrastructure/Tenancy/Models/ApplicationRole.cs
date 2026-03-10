using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Tenancy.Models;

public class ApplicationRole : IdentityRole
{
    public required string Description { get; set; }
}
