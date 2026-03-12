using System.Net;

namespace Application.Exceptions;

public class ForbiddenException(List<string> errorMessages = default!, HttpStatusCode httpStatusCode = HttpStatusCode.Forbidden)
: BaseException(errorMessages, httpStatusCode)
{ }
