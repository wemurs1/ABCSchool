using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Users.Commands;

public class UpdateUserStatusCommand : IRequest<IResponseWrapper>
{
    public required ChangeUserStatusRequest ChangeUserStatus { get; set; }
}

public class UpdateUserStatusCommandHandler(IUserService userService) : IRequestHandler<UpdateUserStatusCommand, IResponseWrapper>
{
    private readonly IUserService _userService = userService;
    public async Task<IResponseWrapper> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = await _userService.ActivateOrDeactivateAsync(request.ChangeUserStatus.UserId, request.ChangeUserStatus.Activation, cancellationToken);
        return ResponseWrapper<string>.Success(data: userId, message: request.ChangeUserStatus.Activation
            ? "User activated successfully" : "User de-activated successfully");
    }
}