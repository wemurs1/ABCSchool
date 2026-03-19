namespace Application.Features.Identity.Roles;

public class CreateRoleRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}
