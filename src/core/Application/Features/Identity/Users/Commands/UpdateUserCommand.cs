using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Users.Commands;

public class UpdateUserCommand : IRequest<IResponseWrapper>
{
    public required UpdateUserRequest UpdateUser { get; set; }
}

public class UpdateUserCommandHandler(IUserService userService) : IRequestHandler<UpdateUserCommand, IResponseWrapper>
{
    private readonly IUserService _userService = userService;

    public async Task<IResponseWrapper> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = await _userService.UpdateAsync(request.UpdateUser, cancellationToken);
        return ResponseWrapper<string>.Success(data: userId, message: "User updated successfully");
    }
}