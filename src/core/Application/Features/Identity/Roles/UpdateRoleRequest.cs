namespace Application.Features.Identity.Roles;

public class UpdateRoleRequest
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}
