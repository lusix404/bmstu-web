//using CoffeeShops.DataAccess.Context;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using CoffeeShops.Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace CoffeeShops.DataAccess.Tests
//{
//    public class FavCoffeeShopsRepositoryTests
//    {
//        private readonly Mock<IDbContextFactory> _mockContextFactory;
//        private readonly IFavCoffeeShopsRepository _repository;

//        public FavCoffeeShopsRepositoryTests()
//        {
//            _mockContextFactory = new Mock<IDbContextFactory>();

//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            var testContext = new CoffeeShopsContext(options);
//            _mockContextFactory.Setup(x => x.GetDbContext(It.IsAny<int>()))
//                .Returns(testContext);

//            _repository = new FavCoffeeShopsRepository(_mockContextFactory.Object);
//        }

//        [Fact]
//        public async Task AddAsync_AddsFavoriteCoffeeShopSuccessfully()
//        {
           
//            var roleId = 3;
//            var userId = Guid.NewGuid();
//            var coffeeShopId = Guid.NewGuid();
//            var favCoffeeShop = new FavCoffeeShops(userId, coffeeShopId);

            
//            await _repository.AddAsync(favCoffeeShop, roleId);

            
//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedFavorite = await context.FavCoffeeShops.FirstOrDefaultAsync();

//            Assert.NotNull(addedFavorite);
//            Assert.Equal(userId, addedFavorite.Id_user);
//            Assert.Equal(coffeeShopId, addedFavorite.Id_coffeeshop);
//        }

//        [Fact]
//        public async Task RemoveAsync_RemovesFavoriteCoffeeShopSuccessfully()
//        {
           
//            var roleId = 3;
//            var userId = Guid.NewGuid();
//            var coffeeShopId = Guid.NewGuid();
//            var favCoffeeShop = new FavCoffeeShops(userId, coffeeShopId);

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            await _repository.AddAsync(favCoffeeShop, roleId);

            
//            await _repository.RemoveAsync(userId, coffeeShopId, roleId);

            
//            var remainingFavorites = await context.FavCoffeeShops
//                .Where(f => f.Id_user == userId && f.Id_coffeeshop == coffeeShopId)
//                .CountAsync();

//            Assert.Equal(0, remainingFavorites);
//        }

//        [Fact]
//        public async Task GetAllFavCoffeeShopsAsync_ReturnsCorrectFavorites()
//        {
           
//            var roleId = 1;
//            var userId = Guid.NewGuid();
//            var testFavorites = new[]
//            {
//                new FavCoffeeShops(userId, Guid.NewGuid()),
//                new FavCoffeeShops(userId, Guid.NewGuid()),
//                new FavCoffeeShops(Guid.NewGuid(), Guid.NewGuid()) 
//            };

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            foreach (var fav in testFavorites)
//            {
//                await _repository.AddAsync(fav, roleId);
//            }

            
//            var result = await _repository.GetAllFavCoffeeShopsAsync(userId, roleId);

            
//            Assert.NotNull(result);
//            Assert.Equal(2, result.Count); // Should only return favorites for specified user
//            Assert.All(result, f => Assert.Equal(userId, f.Id_user));
//        }

//        [Fact]
//        public async Task GetAllFavCoffeeShopsAsync_ReturnsEmptyListForNoFavorites()
//        {
           
//            var roleId = 1;
//            var userId = Guid.NewGuid();

            
//            var result = await _repository.GetAllFavCoffeeShopsAsync(userId, roleId);

            
//            Assert.NotNull(result);
//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task RemoveAsync_DoesNothingWhenFavoriteDoesNotExist()
//        {
           
//            var roleId = 3;
//            var nonExistingUserId = Guid.NewGuid();
//            var nonExistingCoffeeShopId = Guid.NewGuid();

            
//            await _repository.RemoveAsync(nonExistingUserId, nonExistingCoffeeShopId, roleId);

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var count = await context.FavCoffeeShops.CountAsync();
//            Assert.Equal(0, count);
//        }
//    }
//}