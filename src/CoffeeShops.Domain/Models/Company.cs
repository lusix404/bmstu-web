using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CoffeeShops.Domain.Models
{
    public class Company
    {
        public Guid Id_company { get; set; }
        public string Name { get; set; }
        public string? Website { get; set; }

        public Company() { }

        public Company(string _Name, string _Website)
        {
            this.Name = _Name;
            this.Website = _Website;
        }
        public Company(Guid _Id_company,  string _Name, string? _Website)
        {
            this.Id_company = _Id_company;
            this.Name = _Name;
            this.Website = _Website;
        }


        protected bool IsValidWebsite(string? website)
        {
            if (string.IsNullOrEmpty(website))
                return true;
            return Regex.IsMatch(website, @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9-]+\.)*[a-zA-Z0-9-]+\.(ru|su|org|com)(\/[^\s]*)?$");
        }


        public bool Validate()
        {
            return IsValidWebsite(this.Website);
        }
    }
}
