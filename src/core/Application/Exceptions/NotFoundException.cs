using System.Net;

namespace Application.Exceptions;

public class NotFoundException(List<string> errorMessages = default!, HttpStatusCode httpStatusCode = HttpStatusCode.NotFound)
: BaseException(errorMessages, httpStatusCode)
{ }
