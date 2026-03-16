using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Application.Wrappers;

namespace WebApi;

public class ErrorHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var responseWrapper = ResponseWrapper.Fail(message: ex.Message);

            response.StatusCode = ex switch
            {
                ConflictException ce => (int)ce.StatusCode,
                NotFoundException nfe => (int)nfe.StatusCode,
                ForbiddenException fe => (int)fe.StatusCode,
                IdentityException ie => (int)ie.StatusCode,
                UnauthorizedException ue => (int)ue.StatusCode,
                _ => (int)HttpStatusCode.InternalServerError
            };
            var result = JsonSerializer.Serialize(responseWrapper);
            await response.WriteAsync(result);
        }
    }
}
