using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Commands;

public class UpdateRoleCommand : IRequest<IResponseWrapper>
{
    public required UpdateRoleRequest UpdateRole { get; set; }
}

public class UpdateRoleCommandHandler(IRoleService roleService) : IRequestHandler<UpdateRoleCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = await _roleService.UpdateAsync(request.UpdateRole, cancellationToken);
        return ResponseWrapper<string>.Success(data: roleName, message: $"Role '{roleName}' updated sucessfully");
    }
}