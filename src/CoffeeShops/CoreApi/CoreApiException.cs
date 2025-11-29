using System.Net;
using CoffeeShops.DTOs.Utils;

namespace CoffeeShops.CoreApi;

public class CoreApiException : Exception
{
    public int StatusCode { get; }
    public Error? Error { get; }

    public CoreApiException(int statusCode, string message, Error? error = null) : base(message)
    {
        StatusCode = statusCode;
        Error = error;
    }

    public CoreApiException(HttpStatusCode statusCode, string message, Error? error = null)
        : this((int)statusCode, message, error)
    {
    }
}
