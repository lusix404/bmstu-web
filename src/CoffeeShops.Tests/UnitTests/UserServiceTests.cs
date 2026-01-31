using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using Moq;
using Xunit;
using CoffeeShops.Services.Services;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoffeeShops.Domain.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly UserService _userService;
        private readonly ITestOutputHelper _output;

        public UserServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();

            var mockLogger = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _mockUserRepository.Object,
                mockLogger.Object);
        }

        public List<User> GenerateUsers()
        {

            var users = new List<User>()
            {
                new User(Guid.NewGuid(), 1, "blackpink", "whatsupKorea", new DateTime(2003, 12, 31), "a@mail.ru"),
                new User(Guid.NewGuid(), 1, "BTS", "MICDrOP", new DateTime(2013, 12, 31), "bhr@mail.ru"),
                new User(Guid.NewGuid(), 2, "IVE", "lovedive", new DateTime(2000, 12, 31), "dk-3jdno@mail.ru"),
                new User(Guid.NewGuid(), 3, "EXO", "KaiIsExo", new DateTime(1978, 12, 31), "dsldjsi673@mail.ru"),
            };

            foreach (var user in users)
            {
                user.SetPassword(user.PasswordHash != null ? user.PasswordHash : "aaaaaaaaa");
            }

            return users;
        }

        [Fact]
        public async Task GetUserByIdAsync_UserExists()
        {

            var users = GenerateUsers();
            var lastUser = users.Last();


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(lastUser.Id_user, 3))
                .ReturnsAsync(lastUser);

            var result = await _userService.GetUserByIdAsync(lastUser.Id_user, 3);

            Assert.Equal(lastUser, result);
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(lastUser.Id_user, 3), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserNotExists()
        {
            

            var userId = Guid.NewGuid();
            
            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                _userService.GetUserByIdAsync(userId, 3));
            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId, 3), Times.Once);
        }

        [Fact]
        public async Task GetUserByLoginAsync_UserExists()
        {
           

            var login = "EXO";
            var expectedUser = new User(Guid.NewGuid(), 3, login, "KaiIsExo", new DateTime(1978, 12, 31), "dsldjsi673@mail.ru");


            _mockUserRepository.Setup(x => x.GetUserByLoginAsync(login, 3))
                .ReturnsAsync(expectedUser);

            var result = await _userService.GetUserByLoginAsync(login, 3);

            Assert.Equal(expectedUser, result);
            _mockUserRepository.Verify(x => x.GetUserByLoginAsync(login, 3), Times.Once);
        }

        [Fact]
        public async Task GetUserByLoginAsync_UserNotExists()
        {
          

            var login = "nonexistentuser";

            _mockUserRepository.Setup(x => x.GetUserByLoginAsync(login, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserLoginNotFoundException>(() =>
                _userService.GetUserByLoginAsync(login, 3));
            _mockUserRepository.Verify(x => x.GetUserByLoginAsync(login, 3), Times.Once);
        }

        // Get All Users
        [Fact]
        public async Task GetAllUsersAsync_UsersExist()
        {

            var expectedUsers = GenerateUsers();
            _mockUserRepository.Setup(x => x.GetAllUsersAsync(3))
                .ReturnsAsync(expectedUsers);

            var result = await _userService.GetAllUsersAsync(3);

            Assert.Equal(expectedUsers, result);
            _mockUserRepository.Verify(x => x.GetAllUsersAsync(3), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_NoUsers()
        {
            _mockUserRepository.Setup(x => x.GetAllUsersAsync(3))
                .ReturnsAsync(new List<User>());

            // Either modify the service to return empty list instead of throwing,
            // or change the test to expect the exception:
            await Assert.ThrowsAsync<NoUsersFoundException>(() =>
                _userService.GetAllUsersAsync(3));
        }

        [Fact]
        public async Task Login_Success()
        {
            var login = "EXO";
            var password = "KaiIsExo";
            var expectedUser = new User(Guid.NewGuid(), 3, login, "KaiIsExo", new DateTime(1978, 12, 31), "dsldjsi673@mail.ru");
            expectedUser.SetPassword(password); // Make sure password is properly hashed

            _mockUserRepository.Setup(repo => repo.GetUserByLoginAsync(login, It.IsAny<int>()))
                .ReturnsAsync(expectedUser);

            var user = await _userService.Login(login, password);
            Assert.Equal(expectedUser, user);
        }

        [Fact]
        public async Task Login_IncorrectPassword()
        {
            var login = "EXO";
            var password = "wrongpassword";
            var expectedUser = new User(Guid.NewGuid(), 3, login, "correctpassword", new DateTime(1978, 12, 31), "dsldjsi673@mail.ru");
            expectedUser.SetPassword("correctpassword"); // Set correct password

            _mockUserRepository.Setup(repo => repo.GetUserByLoginAsync(login, It.IsAny<int>()))
                .ReturnsAsync(expectedUser);

            await Assert.ThrowsAsync<UserWrongPasswordException>(() =>
                _userService.Login(login, password));
        }

        [Fact]
        public async Task Login_NotFound()
        {

            var login = "nonexistentuser";
            var password = "password";
            var users = GenerateUsers();

            _mockUserRepository.Setup(repo => repo.GetUserByLoginAsync(login, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserLoginNotFoundException>(() =>
                _userService.Login(login, password));
        }

        [Fact]
        public async Task Register_LoginAlreadyExists()
        {
            var user = new User(Guid.NewGuid(), 1, "blackpink", "whatsupKorea", new DateTime(2003, 12, 31), "a@mail.ru");
            var users = GenerateUsers();

            _mockUserRepository.Setup(repo => repo.GetUserByLoginAsync(user.Login, It.IsAny<int>()))
                .ReturnsAsync(users.First()); // Return any existing user

            await Assert.ThrowsAsync<UserLoginAlreadyExistsException>(() =>
                _userService.Registrate(user));
        }

        [Fact]
        public async Task Register_Success()
        {
            var user = new User(Guid.NewGuid(), 1, "newuser", "newpassword", new DateTime(2003, 12, 31), "newuser@mail.ru");
            var users = new List<User>();

            _mockUserRepository.Setup(repo => repo.GetUserByLoginAsync(user.Login, It.IsAny<int>()))
                .ReturnsAsync((User?)null);

            _mockUserRepository.Setup(repo => repo.AddUserAsync(user, It.IsAny<int>()))
                .Callback((User u, int roleId) => users.Add(u))
                .Returns(Task.CompletedTask);

            await _userService.Registrate(user);
            Assert.Single(users); // Check that the user was added
        }
    }
}




