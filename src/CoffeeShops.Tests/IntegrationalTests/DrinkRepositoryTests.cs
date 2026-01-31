using CoffeeShops.DataAccess.Context;
using CoffeeShops.DataAccess.Models;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShops.DataAccess.Repositories;
using Npgsql;

namespace CoffeeShops.DataAccess.Tests.Repositories
{
    public class DrinkRepositoryTests : IDisposable
    {
        private readonly CoffeeShopsContext _context;
        private readonly IDrinkRepository _repository;
        private const string ConnectionString = "Host=localhost;Database=drink_coffeeshops;Username=postgres;Password=lucy2004";

        public DrinkRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            _context = new CoffeeShopsContext(options);

            // Очистка и создание БД
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Заполнение тестовыми данными
            SeedTestData();

            // Инициализация репозитория
            var mockFactory = new Mock<IDbContextFactory>();
            mockFactory.Setup(f => f.GetDbContext(It.IsAny<int>()))
                .Returns(_context);

            _repository = new DrinkRepository(mockFactory.Object);
        }

        private void SeedTestData()
        {
            // Добавляем тестовые напитки
            if (!_context.Drinks.Any())
            {
                _context.Drinks.AddRange(
                    new DrinkDb(Guid.NewGuid(), "Latte"),
                    new DrinkDb(Guid.NewGuid(), "Cappuccino"),
                    new DrinkDb(Guid.NewGuid(), "Espresso")
                );
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetDrinkByIdAsync_ShouldReturnCorrectDrink()
        {
            // Arrange
            var expectedDrink = _context.Drinks.First();

            // Act
            var result = await _repository.GetDrinkByIdAsync(expectedDrink.Id_drink, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDrink.Id_drink, result.Id_drink);
            Assert.Equal(expectedDrink.Name, result.Name);
        }

        [Fact]
        public async Task GetDrinkByIdAsync_ShouldReturnNullForNonExistingDrink()
        {
            // Act
            var result = await _repository.GetDrinkByIdAsync(Guid.NewGuid(), 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllDrinksAsync_ShouldReturnAllDrinks()
        {
            // Arrange
            var expectedCount = _context.Drinks.Count();

            // Act
            var result = await _repository.GetAllDrinksAsync(1);

            // Assert
            Assert.Equal(expectedCount, result.Count);
            Assert.All(result, drink => Assert.NotNull(drink.Name));
        }

       
        
    }
}