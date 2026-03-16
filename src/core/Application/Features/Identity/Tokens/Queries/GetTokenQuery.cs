using Application.Wrappers;
using MediatR;

namespace Application.Features.Identity.Tokens.Queries;

public class GetTokenQuery : IRequest<IResponseWrapper>
{
    public required TokenRequest TokenRequest { get; set; }
}

public class GetTokenQueryHandler(ITokenService tokenService) : IRequestHandler<GetTokenQuery, IResponseWrapper>
{
    private readonly ITokenService _tokenService = tokenService;

    public async Task<IResponseWrapper> Handle(GetTokenQuery request, CancellationToken cancellationToken)
    {
        var tokenResponse = await _tokenService.LoginAsync(request.TokenRequest);
        return await ResponseWrapper<TokenResponse>.SuccessAsynch(tokenResponse);
    }
}