using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Users.Commands;

public class UpdateUserRolesCommand : IRequest<IResponseWrapper>
{
    public required string RoleId { get; set; }
    public required UserRolesRequest UserRolesRequest { get; set; }
}

public class UpdateUserRolesCommandHandler(IUserService userService) : IRequestHandler<UpdateUserRolesCommand, IResponseWrapper>
{
    private readonly IUserService _userService = userService;
    public async Task<IResponseWrapper> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var userId = await _userService.AssignRolesAsync(request.RoleId, request.UserRolesRequest, cancellationToken);
        return ResponseWrapper<string>.Success(data: userId, message: "User roles updated successfully");
    }
}