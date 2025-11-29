using CoffeeShops.Domain.Models;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.User;
using CoffeeShops.DTOs.Auth;

namespace CoffeeShops.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IJwtService jwtService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        //good
        public async Task<User?> GetUserByIdAsync(Guid user_id, int id_role)
        {
            _logger.LogInformation($"Attempting to retrieve user with id={user_id}.");

            var user = await _userRepository.GetUserByIdAsync(user_id, id_role);
            if (user == null)
            {
                _logger.LogWarning($"User with id={user_id} was not found.");
                throw new UserNotFoundException($"User with id={user_id} was not found");
            }

            _logger.LogInformation($"User with id={user_id} retrieved successfully.");
            return user;
        }


        //good
        public async Task<User?> GetUserByLoginAsync(string login, int id_role)
        {
            _logger.LogInformation($"Attempting to retrieve user with login={login}.");

            var user = await _userRepository.GetUserByLoginAsync(login, id_role);
            if (user == null)
            {
                _logger.LogWarning($"User with login={login} was not found.");
                throw new UserLoginNotFoundException($"User with login={login} was not found");
            }

            _logger.LogInformation($"User with login={login} retrieved successfully.");
            return user;
        }


        //modificated
        //возвр. domain.PaginatedResponse[domain.User]
        public async Task<PaginatedResponse<User>>? GetAllUsersAsync(UserFilters filters, int page, int limit, int id_role)
        {
            _logger.LogInformation("Attempting to retrieve all users.");

            (var users, int total) = await _userRepository.GetAllUsersAsync(filters, page, limit, id_role);
            if (users == null || !users.Any())
            {
                _logger.LogWarning("There are no users in the database.");
                throw new NoUsersFoundException("There are no users in the database");
            }

            _logger.LogInformation($"Retrieved {users.Count} users successfully.");
            return new PaginatedResponse<User> { Data = users, Total = total, Limit = limit, Page = page };
        }


        //done - изменен тип возврата на AuthResponse
        public async Task<AuthResponse> Login(string login, string password)
        {
            int id_role = 1;
            _logger.LogInformation($"Attempting to log in user with login={login}.");

            var user = await _userRepository.GetUserByLoginAsync(login, id_role);
            if (user == null)
            {
                _logger.LogWarning($"User with login={login} was not found.");
                throw new UserLoginNotFoundException($"User with login={login} was not found");
            }

            if (!user.VerifyPassword(password))
            {
                _logger.LogWarning($"Incorrect password for login={login}.");
                throw new UserWrongPasswordException($"Password for login={login} is incorrect");
            }

            var token = _jwtService.GenerateToken(user.Id_user, user.Login, UserRoleExtensions.ToRoleIntFromEnumType(user.Id_role));

            _logger.LogInformation($"User with login={login} logged in successfully.");
            return new AuthResponse { Id_user = user.Id_user, Token = token, RoleName = UserRoleExtensions.ToStringFromUserRole(user.Id_role) };
        }


        //исправлено: теперь метод возвр. id пользователя
        public async Task<Guid> Registrate(User user)
        {
            const int dataReadRole = (int)UserRole.Ordinary_user;
            const int dataWriteRole = (int)UserRole.Administrator;

            _logger.LogInformation("Attempting to register user with login={Login}.", user.Login);

            if (await _userRepository.GetUserByLoginAsync(user.Login, dataReadRole) != null)
            {
                _logger.LogWarning($"User with login={user.Login} already exists.");
                throw new UserLoginAlreadyExistsException($"User with login={user.Login} already exists");
            }

            user.SetPassword(user.PasswordHash);

            List<string> validationErrors = user.Validate();
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    _logger.LogWarning($"Validation error for user with login={user.Login}: {error}");
                    throw new UserIncorrectAtributeException(error);
                }
            }

            // Создаем пользователя через контекст с правами записи
            var id_user = await _userRepository.AddUserAsync(user, dataWriteRole);
            _logger.LogInformation("User with login={Login} registered successfully.", user.Login);
            return id_user;
        }

        //возвр. User
        public async Task UpdateUserRightsAsync(Guid id, int new_id_role, int id_role)
        {
            var user = await _userRepository.GetUserByIdAsync(id, id_role);
            if (user == null)
            {
                throw new UserNotFoundException($"User with id={id} was not found");
            }
            await _userRepository.UpdateUserRightsAsync(id, new_id_role, id_role);
        }

        public async Task DeleteUserAsync(Guid id, int id_role)
        {
            var user = await _userRepository.GetUserByIdAsync(id, id_role);
            if (user == null)
            {
                throw new UserNotFoundException($"User with id={id} was not found");
            }
            try
            {
                await _userRepository.DeleteUserAsync(id, id_role);
            }
            catch (UserLastAdminException ex)
            {
                _logger.LogWarning($"Нельзя удалять последнего администратора");
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //возвращает User
        public async Task UpdateUserAsync(User user, int id_role)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(user.Id_user, id_role);
            if (existingUser == null)
            {
                throw new UserNotFoundException($"User with id={user.Id_user} was not found");
            }

            List<string> validationErrors = user.Validate();
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    throw new UserIncorrectAtributeException(error);
                }
            }

            await _userRepository.UpdateUserAsync(user, id_role);
        }



    }
}
