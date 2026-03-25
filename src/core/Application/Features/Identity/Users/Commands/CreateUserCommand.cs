using ABCShared.Library.Models.Requests.Identity;
using ABCShared.Library.Wrappers;
using MediatR;

namespace Application.Features.Identity.Users.Commands;

public class CreateUserCommand : IRequest<IResponseWrapper>
{
    public required CreateUserRequest CreateUser { get; set; }
}

public class CreateUserCommandHandler(IUserService userService) : IRequestHandler<CreateUserCommand, IResponseWrapper>
{
    private readonly IUserService _userService = userService;

    public async Task<IResponseWrapper> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = await _userService.CreateAsync(request.CreateUser, cancellationToken);
        return ResponseWrapper<string>.Success(data: userId, message: "User created successfully");
    }
}