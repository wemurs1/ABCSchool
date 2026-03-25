using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Models.Responses.Identity;

namespace Application.Features.Identity.Roles;

public interface IRoleService
{
    Task<string> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<string> UpdateAsync(UpdateRoleRequest request, CancellationToken ct = default);
    Task<string> DeleteAsync(string id, CancellationToken ct = default);
    Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken ct = default);
    Task<bool> DoesItExistsAsync(string name, CancellationToken ct = default);
    Task<List<RoleResponse>> GetAllAsync(CancellationToken ct = default);
    Task<RoleResponse> GetByIdAsync(string id, CancellationToken ct = default);
    Task<RoleResponse> GetRoleWithPermissionsAsync(string id, CancellationToken ct = default);
}
