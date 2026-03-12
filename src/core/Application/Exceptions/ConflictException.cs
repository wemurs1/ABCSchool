using System.Net;

namespace Application.Exceptions;

public class ConflictException(List<string> errorMessages = default!, HttpStatusCode httpStatusCode = HttpStatusCode.Conflict)
: BaseException(errorMessages, httpStatusCode)
{ }
