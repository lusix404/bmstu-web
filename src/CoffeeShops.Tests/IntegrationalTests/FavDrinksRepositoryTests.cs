//using CoffeeShops.DataAccess.Context;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using CoffeeShops.Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace CoffeeShops.DataAccess.Tests
//{
//    public class FavDrinksRepositoryTests
//    {
//        private readonly Mock<IDbContextFactory> _mockContextFactory;
//        private readonly IFavDrinksRepository _repository;

//        public FavDrinksRepositoryTests()
//        {
//            _mockContextFactory = new Mock<IDbContextFactory>();

            
//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            var testContext = new CoffeeShopsContext(options);
//            _mockContextFactory.Setup(x => x.GetDbContext(It.IsAny<int>()))
//                .Returns(testContext);

//            _repository = new FavDrinksRepository(_mockContextFactory.Object);
//        }

//        [Fact]
//        public async Task AddAsync_AddsFavoriteDrinkSuccessfully()
//        {
            
//            var roleId = 3;
//            var userId = Guid.NewGuid();
//            var drinkId = Guid.NewGuid();
//            var favDrink = new FavDrinks(userId, drinkId);

            
//            await _repository.AddAsync(favDrink, roleId);

            
//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedFavorite = await context.FavDrinks.FirstOrDefaultAsync();

//            Assert.NotNull(addedFavorite);
//            Assert.Equal(userId, addedFavorite.Id_user);
//            Assert.Equal(drinkId, addedFavorite.Id_drink);
//        }

//        [Fact]
//        public async Task RemoveAsync_RemovesFavoriteDrinkSuccessfully()
//        {
            
//            var roleId = 3;
//            var userId = Guid.NewGuid();
//            var drinkId = Guid.NewGuid();
//            var favDrink = new FavDrinks(userId, drinkId);

           
//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            await _repository.AddAsync(favDrink, roleId);

            
//            await _repository.RemoveAsync(userId, drinkId, roleId);

            
//            var remainingFavorites = await context.FavDrinks
//                .Where(f => f.Id_user == userId && f.Id_drink == drinkId)
//                .CountAsync();

//            Assert.Equal(0, remainingFavorites);
//        }

//        [Fact]
//        public async Task GetAllFavDrinksAsync_ReturnsCorrectFavorites()
//        {
            
//            var roleId = 1;
//            var userId = Guid.NewGuid();
//            var testFavorites = new[]
//            {
//                new FavDrinks(userId, Guid.NewGuid()),
//                new FavDrinks(userId, Guid.NewGuid()),
//                new FavDrinks(Guid.NewGuid(), Guid.NewGuid()) // Different user
//            };

            
//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            foreach (var fav in testFavorites)
//            {
//                await _repository.AddAsync(fav, roleId);
//            }

            
//            var result = await _repository.GetAllFavDrinksAsync(userId, roleId);

            
//            Assert.NotNull(result);
//            Assert.Equal(2, result.Count); // Should only return favorites for specified user
//            Assert.All(result, f => Assert.Equal(userId, f.Id_user));
//        }

//        [Fact]
//        public async Task GetAllFavDrinksAsync_ReturnsEmptyListForNoFavorites()
//        {
            
//            var roleId = 1;
//            var userId = Guid.NewGuid();

            
//            var result = await _repository.GetAllFavDrinksAsync(userId, roleId);

            
//            Assert.NotNull(result);
//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task RemoveAsync_DoesNothingWhenFavoriteDoesNotExist()
//        {
            
//            var roleId = 1;
//            var nonExistingUserId = Guid.NewGuid();
//            var nonExistingDrinkId = Guid.NewGuid();

            
//            await _repository.RemoveAsync(nonExistingUserId, nonExistingDrinkId, roleId);

             
//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var count = await context.FavDrinks.CountAsync();
//            Assert.Equal(0, count);
//        }
//    }
//}