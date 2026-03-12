using System.Net;

namespace Application.Exceptions;

public class UnauthorizedException(List<string> errorMessages = default!, HttpStatusCode httpStatusCode = HttpStatusCode.Unauthorized)
: BaseException(errorMessages, httpStatusCode)
{ }