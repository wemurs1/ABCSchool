namespace Application.Features.Identity.Roles;

public class UpdateRolePermissionsRequest
{
    public required string RoleId { get; set; }
    public List<string> NewPermissions { get; set; } = [];
}
