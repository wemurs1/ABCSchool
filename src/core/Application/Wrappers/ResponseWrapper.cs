namespace Application.Wrappers;

public class ResponseWrapper : IResponseWrapper
{
    public List<string> Messages { get; set; } = [];
    public bool IsSuccessful { get; set; }

    // Failure
    public static IResponseWrapper Fail() => Fail([]);
    public static IResponseWrapper Fail(string message) => Fail([message]);
    public static IResponseWrapper Fail(List<string> messages) => new ResponseWrapper() { IsSuccessful = false, Messages = messages };

    public static Task<IResponseWrapper> FailAsynch() => Task.FromResult(Fail());
    public static Task<IResponseWrapper> FailAsynch(string message) => Task.FromResult(Fail(message));
    public static Task<IResponseWrapper> FailAsynch(List<string> messages) => Task.FromResult(Fail(messages));

    // Success
    public static IResponseWrapper Success() => Success([]);
    public static IResponseWrapper Success(string message) => Success([message]);
    public static IResponseWrapper Success(List<string> messages) => new ResponseWrapper() { IsSuccessful = true, Messages = messages };

    public static Task<IResponseWrapper> SuccessAsynch() => Task.FromResult(Success());
    public static Task<IResponseWrapper> SuccessAsynch(string message) => Task.FromResult(Success(message));
    public static Task<IResponseWrapper> SuccessAsynch(List<string> messages) => Task.FromResult(Success(messages));
}

public class ResponseWrapper<T> : IResponseWrapper<T>
{
    public T Data { get; set; } = default!;
    public List<string> Messages { get; set; } = [];
    public bool IsSuccessful { get; set; }

    // Failure
    public static IResponseWrapper<T> Fail() => Fail([]);
    public static IResponseWrapper<T> Fail(string message) => Fail([message]);
    public static IResponseWrapper<T> Fail(List<string> messages) => new ResponseWrapper<T>() { IsSuccessful = false, Messages = messages };

    public static Task<IResponseWrapper<T>> FailAsynch() => Task.FromResult(Fail());
    public static Task<IResponseWrapper<T>> FailAsynch(string message) => Task.FromResult(Fail(message));
    public static Task<IResponseWrapper<T>> FailAsynch(List<string> messages) => Task.FromResult(Fail(messages));

    // Success
    public static IResponseWrapper<T> Success() => Success([]);
    public static IResponseWrapper<T> Success(string message) => Success([message]);
    public static IResponseWrapper<T> Success(List<string> messages) => new ResponseWrapper<T>() { IsSuccessful = true, Messages = messages };

    public static Task<IResponseWrapper<T>> SuccessAsynch() => Task.FromResult(Success());
    public static Task<IResponseWrapper<T>> SuccessAsynch(string message) => Task.FromResult(Success(message));
    public static Task<IResponseWrapper<T>> SuccessAsynch(List<string> messages) => Task.FromResult(Success(messages));

    public static IResponseWrapper<T> Success(T data) => Success(data, []);
    public static IResponseWrapper<T> Success(T data, string message) => Success(data, [message]);
    public static IResponseWrapper<T> Success(T data, List<string> messages) =>
        new ResponseWrapper<T>() { IsSuccessful = true, Messages = messages, Data = data };

    public static Task<IResponseWrapper<T>> SuccessAsynch(T data) => Task.FromResult(Success(data));
    public static Task<IResponseWrapper<T>> SuccessAsynch(T data, string message) => Task.FromResult(Success(data, message));
    public static Task<IResponseWrapper<T>> SuccessAsynch(T data, List<string> messages) => Task.FromResult(Success(data, messages));
}
