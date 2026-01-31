namespace CoffeeShops.Domain.Models
{
    public class FavDrinks
    {
        public Guid Id_user { get; set; }
        public Guid Id_drink { get; set; }
        public string? DrinkName { get; set; }
        public FavDrinks(Guid _Id_drink)
        {
            Id_drink = _Id_drink;
        }
        public FavDrinks(Guid _Id_user, Guid _Id_drink)
        {
            Id_user = _Id_user;
            Id_drink = _Id_drink;
        }
        public FavDrinks(Guid _Id_user, Guid _Id_drink, string _DrinkName)
        {
            Id_user = _Id_user;
            Id_drink = _Id_drink;
            DrinkName = _DrinkName;
        }
    }
}