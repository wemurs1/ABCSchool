using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Users.Queries;

public class UserPermissionsQuery : IRequest<IResponseWrapper>
{
    public required string UserId { get; set; }
}

public class UserPermissionsQueryHandler(IUserService userService) : IRequestHandler<UserPermissionsQuery, IResponseWrapper>
{
    private readonly IUserService _userService = userService;
    public async Task<IResponseWrapper> Handle(UserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _userService.GetUserPermissionsAsync(request.UserId, cancellationToken);
        return ResponseWrapper<List<string>>.Success(permissions);
    }
}