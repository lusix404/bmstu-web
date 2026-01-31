using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Models;

namespace CoffeeShops.DataAccess.Converters;

public static class MenuConverter
{
    public static MenuDb ConvertToDbModel(Menu menu)
    {
        return new MenuDb(
            id_drink: menu.Id_drink,
            id_company: menu.Id_company,
            size: menu.Size,
            price: menu.Price);
    }

    public static Menu ConvertToDomainModel(MenuDb menu)
    {
        return new Menu(
            _Id_drink: menu.Id_drink,
            _Id_company: menu.Id_company,
           _Size: menu.Size,
            _Price: menu.Price);

    }

}

