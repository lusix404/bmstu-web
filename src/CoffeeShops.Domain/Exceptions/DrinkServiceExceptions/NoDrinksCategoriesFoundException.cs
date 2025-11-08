namespace CoffeeShops.Domain.Exceptions.DrinkServiceExceptions
{
    public class NoDrinksCategoriesFoundException : Exception
    {
        public NoDrinksCategoriesFoundException() { }

        public NoDrinksCategoriesFoundException(string? message)
        : base(message) { }

        public NoDrinksCategoriesFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
    }
    
}
