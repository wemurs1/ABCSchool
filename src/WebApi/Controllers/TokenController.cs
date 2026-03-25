using ABCShared.Library.Constants;
using ABCShared.Library.Models.Requests.Token;
using Application.Features.Identity.Tokens.Queries;
using Infrastructure.Identity.Auth;
using Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : BaseApiController
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [TenantHeader]
        [OpenApiOperation("Used to obtain JWT for login")]
        public async Task<IActionResult> GetTokenAsync(TokenRequest request)
        {
            var response = await Sender.Send(new GetTokenQuery { TokenRequest = request });
            if (response.IsSuccessful) return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("refresh-token")]
        [OpenApiOperation("Used to generate a new JWT from refresh token")]
        [TenantHeader]
        [ShouldHavePermission(action: SchoolAction.RefreshToken, feature: SchoolFeature.Tokens)]
        public async Task<IActionResult> GetRefreshTokenAsync(RefreshTokenRequest request)
        {
            var response = await Sender.Send(new GetRefreshTokenQuery { RefreshTokenRequest = request });
            if (response.IsSuccessful) return Ok(response);

            return BadRequest(response);
        }
    }
}
