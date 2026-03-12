using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Models;

public class ApplicationRole : IdentityRole
{
    public required string Description { get; set; }
}
