using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShops.Domain.Exceptions.UserServiceExceptions;

public class UserLastAdminException : Exception
{
    public UserLastAdminException() { }

    public UserLastAdminException(string? message)
    : base(message) { }

    public UserLastAdminException(string? message, Exception? innerException)
    : base(message, innerException) { }
}
