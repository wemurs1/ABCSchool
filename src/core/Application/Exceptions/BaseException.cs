using System.Net;

namespace Application.Exceptions;

public abstract class BaseException(List<string> errorMessages = default!, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : Exception
{
    public List<string> ErrorMessages { get; set; } = errorMessages;
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
