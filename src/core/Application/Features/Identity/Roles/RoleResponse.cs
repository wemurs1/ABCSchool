namespace Application.Features.Identity.Roles;

public class RoleResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<string?> Permissions { get; set; } = [];
}
