using Moq;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoffeeShops.Domain.Tests.Services
{
    public class FavDrinksServiceTests
    {
        private readonly Mock<IFavDrinksRepository> _mockFavDrinksRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IDrinkRepository> _mockDrinkRepository;
        private readonly FavDrinksService _favDrinksService;
        private readonly ITestOutputHelper _output;

        public FavDrinksServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _mockFavDrinksRepository = new Mock<IFavDrinksRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockDrinkRepository = new Mock<IDrinkRepository>();

            var mockLogger = new Mock<ILogger<FavDrinksService>>();

            _favDrinksService = new FavDrinksService(
                _mockFavDrinksRepository.Object,
                _mockUserRepository.Object,
                _mockDrinkRepository.Object,
                mockLogger.Object);

        }

        [Fact]
        public async Task AddDrinkToFavsAsync_Success()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var user = new User { Id_user = userId, FavoriteDrinks = new List<FavDrinks>() };
            var drink = new Drink { Id_drink = Guid.NewGuid() };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(drink);
            _mockFavDrinksRepository.Setup(x => x.AddAsync(It.IsAny<FavDrinks>(), 3))
                .Returns(Task.CompletedTask);

            await _favDrinksService.AddDrinkToFavsAsync(userId, drinkId, 3);

            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId, 3), Times.Once);
            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(drinkId, 3), Times.Once);
            _mockFavDrinksRepository.Verify(x => x.AddAsync(It.Is<FavDrinks>(fd =>
                fd.Id_user == userId && fd.Id_drink == drinkId), 3), Times.Once);
            Assert.Single(user.FavoriteDrinks);
        }

        [Fact]
        public async Task AddDrinkToFavsAsync_UserNotFound()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                _favDrinksService.AddDrinkToFavsAsync(userId, drinkId, 3));

        }

        [Fact]
        public async Task AddDrinkToFavsAsync_DrinkNotFound()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var user = new User { Id_user = userId };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync((Drink?)null);

            await Assert.ThrowsAsync<DrinkNotFoundException>(() =>
                _favDrinksService.AddDrinkToFavsAsync(userId, drinkId, 3));

        }

        [Fact]
        public async Task AddDrinkToFavsAsync_DrinkAlreadyFavorite()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var user = new User
            {
                Id_user = userId,
                FavoriteDrinks = new List<FavDrinks> { new FavDrinks(userId, drinkId) }
            };
            var drink = new Drink { Id_drink = Guid.NewGuid() };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(drink);

            await Assert.ThrowsAsync<DrinkAlreadyIsFavoriteException>(() =>
                _favDrinksService.AddDrinkToFavsAsync(userId, drinkId, 3));

        }

        [Fact]
        public async Task RemoveDrinkFromFavsAsync_Success()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var favorite = new FavDrinks(userId, drinkId);
            var user = new User
            {
                Id_user = userId,
                FavoriteDrinks = new List<FavDrinks> { favorite }
            };
            var drink = new Drink { Id_drink = Guid.NewGuid() };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(drink);
            _mockFavDrinksRepository.Setup(x => x.RemoveAsync(userId, drinkId, 3))
                .Returns(Task.CompletedTask);

            await _favDrinksService.RemoveDrinkFromFavsAsync(userId, drinkId, 3);

            _mockFavDrinksRepository.Verify(x => x.RemoveAsync(userId, drinkId, 3), Times.Once);
            Assert.Empty(user.FavoriteDrinks);

        }

        [Fact]
        public async Task RemoveDrinkFromFavsAsync_UserNotFound()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                _favDrinksService.RemoveDrinkFromFavsAsync(userId, drinkId, 3));

        }

        [Fact]
        public async Task RemoveDrinkFromFavsAsync_DrinkNotFound()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var user = new User { Id_user = userId };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync((Drink?)null);

            await Assert.ThrowsAsync<DrinkNotFoundException>(() =>
                _favDrinksService.RemoveDrinkFromFavsAsync(userId, drinkId, 3));

        }

        [Fact]
        public async Task RemoveDrinkFromFavsAsync_DrinkIsNotFavorite()
        {

            var userId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var user = new User { Id_user = userId, FavoriteDrinks = new List<FavDrinks>() };
            var drink = new Drink { Id_drink = Guid.NewGuid() };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(drink);

            await Assert.ThrowsAsync<DrinkIsNotFavoriteException>(() =>
                _favDrinksService.RemoveDrinkFromFavsAsync(userId, drinkId, 3));
        }

        [Fact]
        public async Task GetFavDrinksAsync_Success()
        {

            var userId = Guid.NewGuid();
            var expectedFavDrinks = new List<FavDrinks>
            {
                new FavDrinks(userId, Guid.NewGuid()),
                new FavDrinks(userId, Guid.NewGuid())
            };
            var user = new User { Id_user = userId };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockFavDrinksRepository.Setup(x => x.GetAllFavDrinksAsync(userId, 3))
                .ReturnsAsync(expectedFavDrinks);

            var result = await _favDrinksService.GetFavDrinksAsync(userId, 3);

            Assert.Equal(expectedFavDrinks, result);
            _mockFavDrinksRepository.Verify(x => x.GetAllFavDrinksAsync(userId, 3), Times.Once);

        }

        [Fact]
        public async Task GetFavDrinksAsync_UserNotFound()
        {

            var userId = Guid.NewGuid();


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                _favDrinksService.GetFavDrinksAsync(userId, 3));

        }

        [Fact]
        public async Task GetFavDrinksAsync_NoFavoriteDrinks()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id_user = userId };


            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockFavDrinksRepository.Setup(x => x.GetAllFavDrinksAsync(userId, 3))
                .ReturnsAsync((List<FavDrinks>?)null);

            await Assert.ThrowsAsync<NoDrinksFoundException>(() =>
                _favDrinksService.GetFavDrinksAsync(userId, 3));

        }
    }
}