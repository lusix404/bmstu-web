using Moq;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Xunit;
using Microsoft.Extensions.Logging;

namespace CoffeeShops.Domain.Tests.Services
{
    public class DrinkServiceTests
    {
        private readonly Mock<IDrinkRepository> _mockDrinkRepository;
        private readonly Mock<IDrinksCategoryRepository> _mockDrinksCategoryRepository;
        private readonly Mock<ILogger<DrinkService>> _mockLogger;
        private readonly DrinkService _drinkService;

        public DrinkServiceTests()
        {
            _mockDrinkRepository = new Mock<IDrinkRepository>();
            _mockDrinksCategoryRepository = new Mock<IDrinksCategoryRepository>();
            _mockLogger = new Mock<ILogger<DrinkService>>();
            _drinkService = new DrinkService(_mockDrinkRepository.Object, _mockDrinksCategoryRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetDrinkByIdAsync_Success()
        {
            var drinkId = Guid.NewGuid();
            var expectedDrink = new Drink { Id_drink = Guid.NewGuid(), Name = "Test Drink" };

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(expectedDrink);

            var result = await _drinkService.GetDrinkByIdAsync(drinkId, 3);

            Assert.Equal(expectedDrink, result);
            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(drinkId, 3), Times.Once);
        }

        [Fact]
        public async Task GetDrinkByIdAsync_DrinkNotFound()
        {
            var invalidId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(invalidId, 3))
                .ReturnsAsync((Drink?)null);

            await Assert.ThrowsAsync<DrinkNotFoundException>(
                () => _drinkService.GetDrinkByIdAsync(invalidId, 3));

            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(invalidId, 3), Times.Once);
        }

        [Fact]
        public async Task GetAllDrinksAsync_Success()
        {
            var expectedDrinks = new List<Drink>
            {
                new Drink { Id_drink = Guid.NewGuid(), Name = "Drink 1" },
                new Drink { Id_drink = Guid.NewGuid(), Name = "Drink 2" }
            };

            _mockDrinkRepository.Setup(x => x.GetAllDrinksAsync(3))
                .ReturnsAsync(expectedDrinks);

            var result = await _drinkService.GetAllDrinksAsync(3);

            Assert.Equal(expectedDrinks, result);
            Assert.Equal(2, result.Count);
            _mockDrinkRepository.Verify(x => x.GetAllDrinksAsync(3), Times.Once);
        }

        [Fact]
        public async Task GetAllDrinksAsync_NoDrinksExist()
        {
            _mockDrinkRepository.Setup(x => x.GetAllDrinksAsync(3))
                .ReturnsAsync((List<Drink>?)null);

            await Assert.ThrowsAsync<NoDrinksFoundException>(
                () => _drinkService.GetAllDrinksAsync(3));

            _mockDrinkRepository.Verify(x => x.GetAllDrinksAsync(3), Times.Once);
        }

        [Fact]
        public async Task AddDrinkAsync_Success()
        {
            var validDrink = new Drink { Id_drink = Guid.NewGuid(), Name = "Valid Drink" };

            _mockDrinkRepository.Setup(x => x.AddAsync(validDrink, 3))
                .Returns(Task.CompletedTask);

            await _drinkService.AddDrinkAsync(validDrink, 3);

            _mockDrinkRepository.Verify(x => x.AddAsync(validDrink, 3), Times.Once);
        }

        [Fact]
        public async Task AddDrinkAsync_EmptyDrinkName()
        {
            var invalidDrink = new Drink { Id_drink = Guid.NewGuid(), Name = "" };

            await Assert.ThrowsAsync<DrinkIncorrectAtributeException>(
                () => _drinkService.AddDrinkAsync(invalidDrink, 3));

            _mockDrinkRepository.Verify(x => x.AddAsync(It.IsAny<Drink>(), 3), Times.Never);
        }

        [Fact]
        public async Task DeleteDrinkAsync_Success()
        {
            var drinkId = Guid.NewGuid();
            var existingDrink = new Drink { Id_drink = drinkId, Name = "Existing Drink" };

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(existingDrink);

            _mockDrinkRepository.Setup(x => x.RemoveAsync(drinkId, 3))
                .Returns(Task.CompletedTask);

            await _drinkService.DeleteDrinkAsync(drinkId, 3);

            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(drinkId, 3), Times.Once);
            _mockDrinkRepository.Verify(x => x.RemoveAsync(drinkId, 3), Times.Once);
        }

        [Fact]
        public async Task DeleteDrinkAsync_DrinkNotFound()
        {
            var invalidId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(invalidId, 3))
                .ReturnsAsync((Drink?)null);

            await Assert.ThrowsAsync<DrinkNotFoundException>(
                () => _drinkService.DeleteDrinkAsync(invalidId, 3));

            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(invalidId, 3), Times.Once);
            _mockDrinkRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), 3), Times.Never);
        }

        [Fact]
        public async Task GetCategoryByDrinkIdAsync_Success()
        {
            var drinkId = Guid.NewGuid();
            var existingDrink = new Drink { Id_drink = drinkId, Name = "Existing Drink" };
            var expectedCategories = new List<Category>
            {
                new Category(Guid.NewGuid(), "Category 1"),
                new Category(Guid.NewGuid(), "Category 2")
            };

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(existingDrink);

            _mockDrinksCategoryRepository.Setup(x => x.GetAllCategoriesByDrinkIdAsync(drinkId, 3))
                .ReturnsAsync(expectedCategories);

            var result = await _drinkService.GetCategoryByDrinkIdAsync(drinkId, 3);

            Assert.Equal(expectedCategories, result);
            Assert.Equal(2, result.Count);
            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(drinkId, 3), Times.Once);
            _mockDrinksCategoryRepository.Verify(x => x.GetAllCategoriesByDrinkIdAsync(drinkId, 3), Times.Once);
        }

        [Fact]
        public async Task GetCategoryByDrinkIdAsync_DrinkNotFound()
        {
            var invalidId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(invalidId, 3))
                .ReturnsAsync((Drink?)null);

            await Assert.ThrowsAsync<DrinkNotFoundException>(
                () => _drinkService.GetCategoryByDrinkIdAsync(invalidId, 3));

            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(invalidId, 3), Times.Once);
            _mockDrinksCategoryRepository.Verify(x => x.GetAllCategoriesByDrinkIdAsync(It.IsAny<Guid>(), 3), Times.Never);
        }
    }
}

