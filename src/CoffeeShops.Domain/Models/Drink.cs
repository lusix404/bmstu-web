using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShops.Domain.Models;

public class Drink
{
    public Guid Id_drink { get; set; }
    public string Name { get; set; }

    public Drink() { }

    public Drink(string _Name)
    {
        this.Name = _Name;
    }
    public Drink(Guid _Id, string _Name)
    {
        this.Id_drink = _Id;
        this.Name = _Name;
    }

    //public ICollection<Category>? Categories { get; set; } = new List<Category>();


}


