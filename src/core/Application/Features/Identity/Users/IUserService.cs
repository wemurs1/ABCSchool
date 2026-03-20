namespace Application.Features.Identity.Users;

public interface IUserService
{
    Task<string> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<string> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<string> DeleteAsync(string userId, CancellationToken cancellationToken = default);
    Task<string> ActivateOrDeactivateAsync(string userId, bool activation, CancellationToken cancellationToken = default);
    Task<string> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<UserRoleResponse>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenAsync(string email, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsPermissionAssigned(string userId, string permission, CancellationToken cancellationToken = default);
}
