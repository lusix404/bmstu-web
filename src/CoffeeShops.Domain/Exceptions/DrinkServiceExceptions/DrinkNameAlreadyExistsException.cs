namespace CoffeeShops.Domain.Exceptions.DrinkServiceExceptions
{
    public class DrinkNameAlreadyExistsException : Exception
    {
        public DrinkNameAlreadyExistsException() { }

        public DrinkNameAlreadyExistsException(string? message)
        : base(message) { }

        public DrinkNameAlreadyExistsException(string? message, Exception? innerException)
        : base(message, innerException) { }
    }
}
