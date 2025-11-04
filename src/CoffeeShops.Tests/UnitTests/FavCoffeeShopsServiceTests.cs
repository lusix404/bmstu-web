using Moq;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CoffeeShops.Domain.Tests.Services
{
    public class FavCoffeeShopsServiceTests
    {
        private readonly Mock<IFavCoffeeShopsRepository> _mockFavCoffeeShopsRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICoffeeShopRepository> _mockCoffeeShopRepository;
        private readonly Mock<ILogger<FavCoffeeShopsService>> _mockLogger;
        private readonly FavCoffeeShopsService _favCoffeeShopsService;

        public FavCoffeeShopsServiceTests()
        {
            _mockFavCoffeeShopsRepository = new Mock<IFavCoffeeShopsRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCoffeeShopRepository = new Mock<ICoffeeShopRepository>();
            _mockLogger = new Mock<ILogger<FavCoffeeShopsService>>();
            _favCoffeeShopsService = new FavCoffeeShopsService(
                _mockFavCoffeeShopsRepository.Object,
                _mockUserRepository.Object,
                _mockCoffeeShopRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task AddCoffeeShopToFavsAsync_Success()
        {
            var userId = Guid.NewGuid();
            var coffeeShopId = Guid.NewGuid();
            var user = new User { Id_user = userId, FavoriteCoffeeShops = new List<FavCoffeeShops>() };
            var coffeeShop = new CoffeeShop { Id_coffeeshop = coffeeShopId };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3))
                .ReturnsAsync(coffeeShop);
            _mockFavCoffeeShopsRepository.Setup(x => x.AddAsync(It.IsAny<FavCoffeeShops>(), 3))
                .Returns(Task.CompletedTask);

            await _favCoffeeShopsService.AddCoffeeShopToFavsAsync(userId, coffeeShopId, 3);

            _mockUserRepository.Verify(x => x.GetUserByIdAsync(userId, 3), Times.Once);
            _mockCoffeeShopRepository.Verify(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3), Times.Once);
            _mockFavCoffeeShopsRepository.Verify(x => x.AddAsync(It.Is<FavCoffeeShops>(f =>
                f.Id_user == userId && f.Id_coffeeshop == coffeeShopId), 3), Times.Once);
            Assert.Single(user.FavoriteCoffeeShops);

        }


        [Fact]
        public async Task AddCoffeeShopToFavsAsync_IsAlreadyFavorite()
        {
            var userId = Guid.NewGuid();
            var coffeeShopId = Guid.NewGuid();
            var user = new User
            {
                Id_user = userId,
                FavoriteCoffeeShops = new List<FavCoffeeShops> { new FavCoffeeShops(userId, coffeeShopId) }
            };
            var coffeeShop = new CoffeeShop { Id_coffeeshop = coffeeShopId };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3))
                .ReturnsAsync(coffeeShop);

            await Assert.ThrowsAsync<CoffeeShopAlreadyIsFavoriteException>(() =>
                _favCoffeeShopsService.AddCoffeeShopToFavsAsync(userId, coffeeShopId, 3));

        }

        //[Fact]
        //public async Task RemoveCoffeeShopFromFavsAsync_Success()
        //{
        //    var userId = Guid.NewGuid();
        //    var coffeeShopId = Guid.NewGuid();
        //    var favorite = new FavCoffeeShops(userId, coffeeShopId);
        //    var user = new User
        //    {
        //        Id_user = userId,
        //        FavoriteCoffeeShops = new List<FavCoffeeShops> { favorite }
        //    };
        //    var coffeeShop = new CoffeeShop { Id_coffeeshop = coffeeShopId };

        //    _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
        //        .ReturnsAsync(user);
        //    _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3))
        //        .ReturnsAsync(coffeeShop);
        //    _mockFavCoffeeShopsRepository.Setup(x => x.RemoveAsync(userId, coffeeShopId, 3))
        //        .Returns(Task.CompletedTask);

        //    await _favCoffeeShopsService.RemoveCoffeeShopFromFavsAsync(userId, coffeeShopId, 3);

        //    _mockFavCoffeeShopsRepository.Verify(x => x.RemoveAsync(userId, coffeeShopId, 3), Times.Once);
        //    Assert.Empty(user.FavoriteCoffeeShops);

        //}

        [Fact]
        public async Task RemoveCoffeeShopFromFavsAsync_UserNotFound()
        {
            var userId = Guid.NewGuid();
            var coffeeShopId = Guid.NewGuid();

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                _favCoffeeShopsService.RemoveCoffeeShopFromFavsAsync(userId, coffeeShopId, 3));

        }

        [Fact]
        public async Task RemoveCoffeeShopFromFavsAsync_CoffeeShopNotFound()
        {
            var userId = Guid.NewGuid();
            var coffeeShopId = Guid.NewGuid();
            var user = new User { Id_user = userId };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3))
                .ReturnsAsync((CoffeeShop?)null);

            await Assert.ThrowsAsync<CoffeeShopNotFoundException>(() =>
                _favCoffeeShopsService.RemoveCoffeeShopFromFavsAsync(userId, coffeeShopId, 3));

        }

     //   [Fact]
     //   public async Task RemoveCoffeeShopFromFavsAsync_NotFavorite()
     //   {
     //       var userId = Guid.NewGuid();
     //       var coffeeShopId = Guid.NewGuid();
     //       var user = new User { Id_user = userId, FavoriteCoffeeShops = new List<FavCoffeeShops>() };
     //       var coffeeShop = new CoffeeShop { Id_coffeeshop = coffeeShopId };

     //       _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
     //           .ReturnsAsync(user);
     //       _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3))
     //           .ReturnsAsync(coffeeShop);

     //       await Assert.ThrowsAsync<CoffeeShopIsNotFavoriteException>(() =>
     //_favCoffeeShopsService.RemoveCoffeeShopFromFavsAsync(userId, coffeeShopId, 3));

     //   }

        [Fact]
        public async Task GetFavCoffeeShopsAsync_Success()
        {
            var userId = Guid.NewGuid();
            var expectedFavorites = new List<FavCoffeeShops>
            {
                new FavCoffeeShops(userId, Guid.NewGuid()),
                new FavCoffeeShops(userId, Guid.NewGuid())
            };
            var user = new User { Id_user = userId };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockFavCoffeeShopsRepository.Setup(x => x.GetAllFavCoffeeShopsAsync(userId, 3))
                .ReturnsAsync(expectedFavorites);

            var result = await _favCoffeeShopsService.GetFavCoffeeShopsAsync(userId, 3);

            Assert.Equal(expectedFavorites, result);
            _mockFavCoffeeShopsRepository.Verify(x => x.GetAllFavCoffeeShopsAsync(userId, 3), Times.Once);
        }

        [Fact]
        public async Task GetFavCoffeeShopsAsync_UserNotFound()
        {
            var userId = Guid.NewGuid();

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                _favCoffeeShopsService.GetFavCoffeeShopsAsync(userId, 3));

        }

        [Fact]
        public async Task GetFavCoffeeShopsAsync_NoFavorites()
        {
            var userId = Guid.NewGuid();
            var user = new User { Id_user = userId };

            _mockUserRepository.Setup(x => x.GetUserByIdAsync(userId, 3))
                .ReturnsAsync(user);
            _mockFavCoffeeShopsRepository.Setup(x => x.GetAllFavCoffeeShopsAsync(userId, 3))
                .ReturnsAsync((List<FavCoffeeShops>?)null);

            await Assert.ThrowsAsync<NoCoffeeShopsFoundException>(() =>
                _favCoffeeShopsService.GetFavCoffeeShopsAsync(userId, 3));

        }
    }
}




