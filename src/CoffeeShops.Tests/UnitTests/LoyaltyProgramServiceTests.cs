using Moq;
using CoffeeShops.Domain.Exceptions.LoyaltyProgramServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CoffeeShops.Domain.Tests.Services
{
    public class LoyaltyProgramServiceTests
    {
        private readonly Mock<ILoyaltyProgramRepository> _mockLpRepository;
        private readonly Mock<ILogger<LoyaltyProgramService>> _mockLogger;
        private readonly LoyaltyProgramService _lpService;

        public LoyaltyProgramServiceTests()
        {
            _mockLpRepository = new Mock<ILoyaltyProgramRepository>();
            _mockLogger = new Mock<ILogger<LoyaltyProgramService>>();
            _lpService = new LoyaltyProgramService(_mockLpRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLoyaltyProgramByIdAsync_Success()
        {
            var lpId = Guid.NewGuid();
            var expectedLp = new LoyaltyProgram(lpId, "Test Loyalty Program");

            _mockLpRepository.Setup(x => x.GetLoyaltyProgramByIdAsync(lpId, 3))
                .ReturnsAsync(expectedLp);

            var result = await _lpService.GetLoyaltyProgramByIdAsync(lpId, 3);

            Assert.Equal(expectedLp, result);
            Assert.Equal(lpId, result.Id_lp);
            Assert.Equal("Test Loyalty Program", result.Description);
            _mockLpRepository.Verify(x => x.GetLoyaltyProgramByIdAsync(lpId, 3), Times.Once);
        }

        [Fact]
        public async Task GetLoyaltyProgramByIdAsync_ProgramNotFound()
        {
            var invalidId = Guid.NewGuid();

            _mockLpRepository.Setup(x => x.GetLoyaltyProgramByIdAsync(invalidId, 3))
                .ReturnsAsync((LoyaltyProgram?)null);

            var exception = await Assert.ThrowsAsync<LoyaltyProgramNotFoundException>(
                () => _lpService.GetLoyaltyProgramByIdAsync(invalidId, 3));

            Assert.Equal($"LoyaltyProgram with id={invalidId} was not found", exception.Message);
            _mockLpRepository.Verify(x => x.GetLoyaltyProgramByIdAsync(invalidId, 3), Times.Once);
        }
    }
}
