using Moq;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CoffeeShops.Domain.Tests.Services
{
    public class CoffeeShopServiceTests
    {
        private readonly Mock<ICoffeeShopRepository> _mockCoffeeShopRepository;
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<ILogger<CoffeeShopService>> _mockLogger;
        private readonly CoffeeShopService _coffeeShopService;

        public CoffeeShopServiceTests()
        {
            _mockCoffeeShopRepository = new Mock<ICoffeeShopRepository>();
            _mockCompanyRepository = new Mock<ICompanyRepository>();
            _mockLogger = new Mock<ILogger<CoffeeShopService>>();
            _coffeeShopService = new CoffeeShopService(
                _mockCoffeeShopRepository.Object,
                _mockCompanyRepository.Object,
                _mockLogger.Object);
        }

        
        [Fact]
        public async Task AddCoffeeShopAsync_Success()
        {
            var companyId = Guid.NewGuid();
            var coffeeShop = new CoffeeShop(
                Guid.NewGuid(),
                companyId,
                "123 Main St",
                "9:00-18:00");

            var company = new Company { Id_company= companyId };

            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(companyId, 3))
                .ReturnsAsync(company);
            _mockCoffeeShopRepository.Setup(x => x.AddAsync(coffeeShop, 3))
                .Returns(Task.CompletedTask);

            await _coffeeShopService.AddCoffeeShopAsync(coffeeShop, 3);

            _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId, 3), Times.Once);
            _mockCoffeeShopRepository.Verify(x => x.AddAsync(coffeeShop, 3), Times.Once);
        }

        [Fact]
        public async Task AddCoffeeShopAsync_EmptyAddress()
        {
            var coffeeShop = new CoffeeShop(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "",
                "9:00-18:00");

            var exception = await Assert.ThrowsAsync<CoffeeShopIncorrectAtributeException>(
                () => _coffeeShopService.AddCoffeeShopAsync(coffeeShop, 3));

            Assert.Equal("Coffeeshop's address cannot be empty", exception.Message);
        }

        [Fact]
        public async Task AddCoffeeShopAsync_CompanyNotFound()
        {
            var invalidCompanyId = Guid.NewGuid();
            var coffeeShop = new CoffeeShop(
                Guid.NewGuid(),
                invalidCompanyId,
                "123 Main St",
                "9:00-18:00");

            _mockCompanyRepository.Setup(x => x.GetCompanyByIdAsync(invalidCompanyId, 3))
                .ReturnsAsync((Company?)null);

            var exception = await Assert.ThrowsAsync<CompanyNotFoundException>(
                () => _coffeeShopService.AddCoffeeShopAsync(coffeeShop, 3));

            Assert.Equal($"Company with id={invalidCompanyId} was not found", exception.Message);
            _mockCoffeeShopRepository.Verify(x => x.AddAsync(It.IsAny<CoffeeShop>(), 3), Times.Never);
        }

        [Fact]
        public async Task DeleteCoffeeShopAsync_Success()
        {
            var coffeeShopId = Guid.NewGuid();
            var existingCoffeeShop = new CoffeeShop(
                coffeeShopId,
                Guid.NewGuid(),
                "123 Main St",
                "9:00-18:00");

            _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3))
                .ReturnsAsync(existingCoffeeShop);
            _mockCoffeeShopRepository.Setup(x => x.RemoveAsync(coffeeShopId, 3))
                .Returns(Task.CompletedTask);

            await _coffeeShopService.DeleteCoffeeShopAsync(coffeeShopId, 3);

            _mockCoffeeShopRepository.Verify(x => x.GetCoffeeShopByIdAsync(coffeeShopId, 3), Times.Once);
            _mockCoffeeShopRepository.Verify(x => x.RemoveAsync(coffeeShopId, 3), Times.Once);
        }

        [Fact]
        public async Task DeleteCoffeeShopAsync_CoffeeShopNotFound()
        {
            var invalidId = Guid.NewGuid();

            _mockCoffeeShopRepository.Setup(x => x.GetCoffeeShopByIdAsync(invalidId, 3))
                .ReturnsAsync((CoffeeShop?)null);

            var exception = await Assert.ThrowsAsync<CoffeeShopNotFoundException>(
                () => _coffeeShopService.DeleteCoffeeShopAsync(invalidId, 3));

            Assert.Equal($"Coffeeshop with id={invalidId} was not found", exception.Message);
            _mockCoffeeShopRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), 3), Times.Never);
        }
    }
}
