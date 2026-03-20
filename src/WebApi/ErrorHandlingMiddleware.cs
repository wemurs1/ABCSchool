using System.Net;
using System.Net.Mime;
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
            response.ContentType = MediaTypeNames.Application.Json;

            var responseWrapper = ResponseWrapper.Fail();

            switch (ex)
            {
                case ConflictException ce:
                    response.StatusCode = (int)ce.StatusCode;
                    responseWrapper.Messages = ce.ErrorMessages;
                    break;
                case NotFoundException nfe:
                    response.StatusCode = (int)nfe.StatusCode;
                    responseWrapper.Messages = nfe.ErrorMessages;
                    break;
                case ForbiddenException fe:
                    response.StatusCode = (int)fe.StatusCode;
                    responseWrapper.Messages = fe.ErrorMessages;
                    break;
                case IdentityException ie:
                    response.StatusCode = (int)ie.StatusCode;
                    responseWrapper.Messages = ie.ErrorMessages;
                    break;
                case UnauthorizedException ue:
                    response.StatusCode = (int)ue.StatusCode;
                    responseWrapper.Messages = ue.ErrorMessages;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseWrapper.Messages = ["Something went wrong. Contact your Administrator", ex.Message];
                    break;
            }

            var result = JsonSerializer.Serialize(responseWrapper);
            await response.WriteAsync(result);
        }
    }
}
