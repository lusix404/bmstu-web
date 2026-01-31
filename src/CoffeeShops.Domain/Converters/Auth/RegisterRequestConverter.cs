using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CoffeeShops.DTOs.Auth;
using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.Domain.Converters;


public static class RegisterRequestConverter
{
    public static User ConvertToDomainModel(RegisterRequest r)
    {
        return new User(
            _Id_role: UserRole.Ordinary_user,
            _Login: r.Login,
            _Passwordhash: r.Password,
            _BirthDate: r.BirthDate,
            _Email: r.Email);
    }
}