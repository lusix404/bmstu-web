using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoffeeShops.DTOs.Utils;

public class Error
{
    public string Message { get; set; }
    public int Code { get; set; }
    public Exception? Err { get; set; }
    public string? ErrorDetails { get; set; }

    public Error() { }

    public Error(string message, int code, Exception err = null)
    {
        Message = message;
        Code = code;
        Err = err;
        ErrorDetails = err?.Message;
    }

    public Error(string message, Exception err = null)
    {
        Message = message;
        Code = 0; 
        Err = err;
        ErrorDetails = err?.Message;
    }

    public Error(string message, int code, string err_details)
    {
        Message = message;
        Code = code;
        ErrorDetails = err_details;
    }
}

public static class ErrorExtensions
{
    public static Error NewBadRequest(string msg, Exception err = null)
    {
        return new Error(msg, 400, err); // 400 - StatusBadRequest
    }

    public static Error NewNotFound(string msg, Exception err = null)
    {
        return new Error(msg, 404, err); // 404 - StatusNotFound
    }

    public static Error NewInternalServerError(string msg, Exception err = null)
    {
        return new Error(msg, 500, err);
    }

    public static Error NewUnauthorized(string msg, Exception err = null)
    {
        return new Error(msg, 401, err);
    }

    public static Error NewForbidden(string msg, Exception err = null)
    {
        return new Error(msg, 403, err);
    }
}