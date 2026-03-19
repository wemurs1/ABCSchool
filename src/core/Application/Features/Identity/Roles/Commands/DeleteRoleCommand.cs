using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Commands;

public class DeleteRoleCommand : IRequest<IResponseWrapper>
{
    public required string RoleId { get; set; }
}

public class DeleteRoleCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRoleCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = await _roleService.DeleteAsync(request.RoleId, cancellationToken);
        return ResponseWrapper<string>.Success(data: roleName, message: $"Role '{roleName}' deleted sucessfully");
    }
}