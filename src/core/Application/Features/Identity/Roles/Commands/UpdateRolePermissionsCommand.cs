using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Commands;

public class UpdateRolePermissionsCommand : IRequest<IResponseWrapper>
{
    public required UpdateRolePermissionsRequest UpdateRolePermissions { get; set; }
}

public class UpdateRolePermissionsCommandHandler(IRoleService roleService) : IRequestHandler<UpdateRolePermissionsCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var message = await _roleService.UpdatePermissionsAsync(request.UpdateRolePermissions, cancellationToken);
        return ResponseWrapper<string>.Success(message: message);
    }
}