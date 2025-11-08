using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using Npgsql;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.User;

namespace CoffeeShops.DataAccess.Repositories;
public class UserRepository : IUserRepository
{
    private IDbContextFactory _contextFactory;
    public UserRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Guid> AddUserAsync(User user, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var userDb = UserConverter.ConvertToDbModel(user);
        await _context.AddAsync(userDb);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var innerException = ex.InnerException;

            if (innerException != null)
            {
                Console.WriteLine($"Inner Exception: {innerException.Message}");
            }
            else
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        var added_user = await _context.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
        return added_user.Id_user;
        //await _context.SaveChangesAsync();
    }

    public async Task RemoveUserAsync(Guid user_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var userDb = await _context.Users.FindAsync(user_id);
        if (userDb != null)
        {
            _context.Users.Remove(userDb);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserByLoginAsync(string login, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
        return user != null ? UserConverter.ConvertToDomainModel(user) : null;
    }

    public async Task<User?> GetUserByIdAsync(Guid user_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var user = await _context.Users.FindAsync(user_id);
        return user != null ? UserConverter.ConvertToDomainModel(user) : null;
    }

    public async Task<(List<User>? data, int total)> GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        //var users = await _context.Users
        //        .AsNoTracking()
        //        .ToListAsync();

        //return users.ConvertAll(user => UserConverter.ConvertToDomainModel(user));


        //преобразование к типу IQueryable<T> - интерфейс, который позволяет строить деревья
        //выражений (Expression Trees), которые выполняются на стороне базы данных, а не в памяти приложения.
        var query = _context.Users.AsQueryable();
        if (!string.IsNullOrEmpty(filters.Login))
        {
            query = query.Where(u => u.Login == filters.Login);
        }
        if (filters.UserRole != null)
        {
            query = query.Where(u => u.Id_role == (int)filters.UserRole);
        }
        
        var total = await query.CountAsync(); //количество элементов, удовлетворяющих условиям

        //var to_skip = (total <= (page - 1) * limit) ? 0 : ((page - 1) * limit);

        //var users = await query
        //    .OrderBy(u => u.Login)
        //    .Skip(to_skip)
        //    .Take(limit)
        //    .ToListAsync();

        var users = await query
            .OrderBy(u => u.Login)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (users.ConvertAll(user => UserConverter.ConvertToDomainModel(user)), total);
    }

    public async Task UpdateUserAsync(User user, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        var existingUser = await _context.Users.FindAsync(user.Id_user);

        if (existingUser == null)
        {
            throw new UserNotFoundException($"User with id={user.Id_user} not found");
        }

        existingUser.Login = user.Login;
        existingUser.Email = user.Email;
        existingUser.Password = user.PasswordHash;
        existingUser.BirthDate = user.BirthDate;
        existingUser.Id_role = UserRoleExtensions.ToRoleIntFromEnumType(user.Id_role);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new UserNotFoundException($"User with id={user.Id_user} not found");
        }
    }

    public async Task PartialUpdateUserAsync(Guid Id_user, string login, string password, string email, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        var existingUser = await _context.Users.FindAsync(Id_user);

        if (existingUser == null)
        {
            throw new UserNotFoundException($"User with id={Id_user} not found");
        }

        existingUser.Login = login;
        existingUser.Email = email;
        existingUser.Password = password;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new UserNotFoundException($"User with id={Id_user} not found");
        }
    }

    public async Task UpdateUserRightsAsync(Guid id, int new_role, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            throw new UserNotFoundException($"User with id={id} not found");
        }


        user.Id_role = new_role;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex) { }
    }

    public async Task DeleteUserAsync(Guid id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            throw new UserNotFoundException($"User with id={id} not found");
        }
        if (user.Id_role == (int)UserRole.Administrator)
        {
            var adminCount = await _context.Users
           .CountAsync(u => u.Id_role == (int)UserRole.Administrator);

            if (adminCount <= 1)
            {
                throw new UserLastAdminException("Нельзя удалить аккаунт последнего администратора");
            }
        }
        //var adminCount = await _context.Users.CountAsync(u => u.Id_role == (int)UserRole.Administrator);
        //if (adminCount == 1 && user.Id_role == (int)UserRole.Administrator)
        //{
        //    throw new UserLastAdminException("Нельзя удалить аккаунт последнего администратора.");
        //}


        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}

