using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Roles.Commands;

public class CreateRoleCommand : IRequest<IResponseWrapper>
{
    public required CreateRoleRequest CreateRole { get; set; }
}

public class CreateRoleCommandHandler(IRoleService roleService) : IRequestHandler<CreateRoleCommand, IResponseWrapper>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<IResponseWrapper> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = await _roleService.CreateAsync(request.CreateRole, cancellationToken);
        return ResponseWrapper<string>.Success(data: roleName, message: $"Role '{roleName}' created sucessfully");
    }
}