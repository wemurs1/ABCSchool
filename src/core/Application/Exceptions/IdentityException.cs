using System.Net;

namespace Application.Exceptions;

public class IdentityException(List<string> errorMessages = default!, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
: BaseException(errorMessages, httpStatusCode)
{ }
