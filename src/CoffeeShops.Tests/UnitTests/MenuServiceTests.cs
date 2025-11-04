using Moq;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.MenuServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Microsoft.Extensions.Logging; // Add this using directive
using Xunit;

namespace CoffeeShops.Domain.Tests.Services
{
    public class MenuServiceTests
    {
        private readonly Mock<IMenuRepository> _mockMenuRepository;
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<IDrinkRepository> _mockDrinkRepository;
        private readonly Mock<ILogger<MenuService>> _mockLogger; // Mock for ILogger
        private readonly MenuService _menuService;

        public MenuServiceTests()
        {
            _mockMenuRepository = new Mock<IMenuRepository>();
            _mockCompanyRepository = new Mock<ICompanyRepository>();
            _mockDrinkRepository = new Mock<IDrinkRepository>();
            _mockLogger = new Mock<ILogger<MenuService>>(); // Initialize the logger mock
            _menuService = new MenuService(
                _mockMenuRepository.Object,
                _mockCompanyRepository.Object,
                _mockDrinkRepository.Object,
                _mockLogger.Object); // Pass the logger to the MenuService
        }

        [Fact]
        public async Task GetMenuByCompanyIdAsync_Success()
        {
            var companyId = Guid.NewGuid();
            var expectedMenu = new List<Menu>
            {
                new Menu(Guid.NewGuid(), companyId, 300, 5.99m),
                new Menu(Guid.NewGuid(), companyId, 400, 4.99m)
            };

            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(companyId, 3))
                .ReturnsAsync(new Company());
            _mockMenuRepository.Setup(x => x.GetMenuByCompanyId(companyId, 3))
                .ReturnsAsync(expectedMenu);

            var result = await _menuService.GetMenuByCompanyIdAsync(companyId, 3);

            Assert.Equal(expectedMenu, result);
            _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId, 3), Times.Once);
            _mockMenuRepository.Verify(x => x.GetMenuByCompanyId(companyId, 3), Times.Once);
        }

        [Fact]
        public async Task GetMenuByCompanyIdAsync_CompanyNotFound()
        {
            var invalidCompanyId = Guid.NewGuid();

            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(invalidCompanyId, 3))
                .ReturnsAsync((Company?)null);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(
                () => _menuService.GetMenuByCompanyIdAsync(invalidCompanyId, 3));

            Assert.Equal($"Company with id={invalidCompanyId} was not found", exception.Message);
        }

        [Fact]
        public async Task GetMenuByCompanyIdAsync_MenuNotFound()
        {
            var companyId = Guid.NewGuid();

            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(companyId, 3))
                .ReturnsAsync(new Company());
            _mockMenuRepository.Setup(x => x.GetMenuByCompanyId(companyId, 3))
                .ReturnsAsync((List<Menu>?)null);

            var exception = await Assert.ThrowsAsync<MenuNotFoundException>(
                () => _menuService.GetMenuByCompanyIdAsync(companyId, 3));


            Assert.Equal($"Menu for company with id={companyId} was not found", exception.Message);
        }

        [Fact]
        public async Task GetCompaniesByDrinkIdAsync_Success()
        {
            var drinkId = Guid.NewGuid();
            var expectedCompanies = new List<Company>
            {
                new Company(Guid.NewGuid(), Guid.NewGuid(), "Company 1", "https://company1.com", 3),
                new Company(Guid.NewGuid(), Guid.NewGuid(), "Company 2", "https://company2.com", 5)
            };

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(new Drink());
            _mockMenuRepository.Setup(x => x.GetCompaniesByDrinkIdAsync(drinkId, 3))
                .ReturnsAsync(expectedCompanies);

            var result = await _menuService.GetCompaniesByDrinkIdAsync(drinkId, 3);

            Assert.Equal(expectedCompanies, result);
            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(drinkId, 3), Times.Once);
            _mockMenuRepository.Verify(x => x.GetCompaniesByDrinkIdAsync(drinkId, 3), Times.Once);
        }

        [Fact]
        public async Task GetCompaniesByDrinkIdAsync_DrinkNotFound()
        {
            var invalidDrinkId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(invalidDrinkId, 3))
                .ReturnsAsync((Drink?)null);

            var exception = await Assert.ThrowsAsync<DrinkNotFoundException>(
                () => _menuService.GetCompaniesByDrinkIdAsync(invalidDrinkId, 3));



            Assert.Equal($"Drink with id={invalidDrinkId} was not found", exception.Message);
        }

        [Fact]
        public async Task AddDrinkToMenuAsync_Success()
        {
            var validMenu = new Menu(
                Guid.NewGuid(),
                Guid.NewGuid(),
                500,
                5.99m);

            _mockMenuRepository.Setup(x => x.AddAsync(validMenu, 3))
                .Returns(Task.CompletedTask);

            await _menuService.AddDrinkToMenuAsync(validMenu, 3);

            _mockMenuRepository.Verify(x => x.AddAsync(validMenu, 3), Times.Once);
        }

        [Fact]
        public async Task AddDrinkToMenuAsync_DrinkSizeInvalid()
        {
            var invalidMenu = new Menu(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                5.99m);

            var exception = await Assert.ThrowsAsync<MenuIncorrectAtributeException>(
                () => _menuService.AddDrinkToMenuAsync(invalidMenu, 3));


            Assert.Equal("Drink's size in menu name cannot be empty", exception.Message);
        }

        [Fact]
        public async Task AddDrinkToMenuAsync_InvalidPrice()
        {
            var invalidMenu = new Menu(
                Guid.NewGuid(),
                Guid.NewGuid(),
                300,
                0);

            var exception = await Assert.ThrowsAsync<MenuIncorrectAtributeException>(
                () => _menuService.AddDrinkToMenuAsync(invalidMenu, 3));


            Assert.Equal("Drink's price in menu must be > 0", exception.Message);
        }

        [Fact]
        public async Task DeleteDrinkFromMenuAsync_Success()
        {
            var drinkId = Guid.NewGuid();
            var companyId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(new Drink());
            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(companyId, 3))
                .ReturnsAsync(new Company());
            _mockMenuRepository.Setup(x => x.RemoveAsync(drinkId, companyId, 3))
                .Returns(Task.CompletedTask);

            await _menuService.DeleteDrinkFromMenuAsync(drinkId, companyId, 3);

            _mockDrinkRepository.Verify(x => x.GetDrinkByIdAsync(drinkId, 3), Times.Once);
            _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId, 3), Times.Once);
            _mockMenuRepository.Verify(x => x.RemoveAsync(drinkId, companyId, 3), Times.Once);
        }

        [Fact]
        public async Task DeleteDrinkFromMenuAsync_DrinkNotFound()
        {
            var invalidDrinkId = Guid.NewGuid();
            var companyId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(invalidDrinkId, 3))
                .ReturnsAsync((Drink?)null);

            var exception = await Assert.ThrowsAsync<DrinkNotFoundException>(
     () => _menuService.DeleteDrinkFromMenuAsync(invalidDrinkId, companyId, 3));


            Assert.Equal($"Drink with id={invalidDrinkId} was not found", exception.Message);
        }

        [Fact]
        public async Task DeleteDrinkFromMenuAsync_CompanyNotFound()
        {
            var drinkId = Guid.NewGuid();
            var invalidCompanyId = Guid.NewGuid();

            _mockDrinkRepository.Setup(x => x.GetDrinkByIdAsync(drinkId, 3))
                .ReturnsAsync(new Drink());
            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(invalidCompanyId, 3))
                .ReturnsAsync((Company?)null);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(
                () => _menuService.DeleteDrinkFromMenuAsync(drinkId, invalidCompanyId, 3));


            Assert.Equal($"Company with id={invalidCompanyId} was not found", exception.Message);
        }
    }
}


