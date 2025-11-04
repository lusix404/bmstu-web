using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CoffeeShops.Domain.Models
{
    public class Company
    {
        public Guid Id_company { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
        public Company(string _Name, string _Website)
        {
            this.Name = _Name;
            this.Website = _Website;
        }
        public Company(Guid _Id_company, Guid _Id_lp, string _Name, string _Website)
        {
            this.Id_company = _Id_company;
            this.Name = _Name;
            this.Website = _Website;
        }


        protected bool IsValidWebsite(string website)
        {
            return Regex.IsMatch(website, @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9-]+\.)*[a-zA-Z0-9-]+\.(ru|su|org|com)(\/[^\s]*)?$");
        }

        protected bool IsValidAmountCOffeeShops(int amount)
        {
            return amount > 0;
        }

        public bool Validate()
        {
            return IsValidWebsite(this.Website) & IsValidAmountCOffeeShops(this.AmountCoffeeShops);
        }
    }
}