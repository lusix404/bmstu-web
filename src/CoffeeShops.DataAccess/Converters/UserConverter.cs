using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Models;
using CoffeeShops.Domain.Models.Enums;

namespace CoffeeShops.DataAccess.Converters;

public static class UserConverter
{
        public static UserDb ConvertToDbModel(User user)
        {
            return new UserDb(
                id_user: user.Id_user,
                id_role: UserRoleExtensions.ToRoleIntFromEnumType(user.Id_role),
                login: user.Login,
                password: user.PasswordHash ?? "",
                birthDate: user.BirthDate,
                email: user.Email);
        }


        public static User ConvertToDomainModel(UserDb user)
        {
            return new User(
                _Id: user.Id_user,
                _Id_role: UserRoleExtensions.ToUserRoleEnumTypeFromInt(user.Id_role),
                _Login: user.Login,
                _Passwordhash: user.Password,
                _BirthDate: user.BirthDate,
                _Email: user.Email);
    }


    }
