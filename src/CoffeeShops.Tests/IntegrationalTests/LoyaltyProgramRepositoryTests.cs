//using CoffeeShops.DataAccess.Context;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using CoffeeShops.Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace CoffeeShops.DataAccess.Tests
//{
//    public class LoyaltyProgramRepositoryTests
//    {
//        private readonly Mock<IDbContextFactory> _mockContextFactory;
//        private readonly ILoyaltyProgramRepository _repository;

//        public LoyaltyProgramRepositoryTests()
//        {
//            _mockContextFactory = new Mock<IDbContextFactory>();

//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            var testContext = new CoffeeShopsContext(options);
//            _mockContextFactory.Setup(x => x.GetDbContext(It.IsAny<int>()))
//                .Returns(testContext);

//            _repository = new LoyaltyProgramRepository(_mockContextFactory.Object);
//        }

//        [Fact]
//        public async Task GetLoyaltyProgramByIdAsync_ReturnsCorrectProgram()
//        {
//            var roleId = 1;
//            var programId = Guid.NewGuid();
//            var testProgram = new LoyaltyProgram(programId, "Premium loyalty program");

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            await _repository.AddAsync(testProgram, roleId);

//            var result = await _repository.GetLoyaltyProgramByIdAsync(programId, roleId);

//            Assert.NotNull(result);
//            Assert.Equal(programId, result.Id_lp);
//            Assert.Equal("Premium loyalty program", result.Description);
//        }

//        [Fact]
//        public async Task GetLoyaltyProgramByIdAsync_ReturnsNullForNonExistingProgram()
//        {
//            var roleId = 1;
//            var nonExistingId = Guid.NewGuid();

//            var result = await _repository.GetLoyaltyProgramByIdAsync(nonExistingId, roleId);

//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task AddAsync_AddsLoyaltyProgramSuccessfully()
//        {
//            var roleId = 3;
//            var program = new LoyaltyProgram(Guid.NewGuid(), "Test Program");

//            await _repository.AddAsync(program, roleId);

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedProgram = await context.LoyaltyPrograms.FirstOrDefaultAsync();
//            Assert.NotNull(addedProgram);
//            Assert.Equal("Test Program", addedProgram.Description);
//        }

//        [Fact]
//        public async Task RemoveAsync_DeletesLoyaltyProgramSuccessfully()
//        {
//            var roleId = 3;
//            var programId = Guid.NewGuid();
//            var testProgram = new LoyaltyProgram(programId, "To Delete");

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            await _repository.AddAsync(testProgram, roleId);

//            await _repository.RemoveAsync(programId, roleId);

//            var deletedProgram = await context.LoyaltyPrograms.FindAsync(programId);
//            Assert.Null(deletedProgram);
//        }

//        [Fact]
//        public async Task AddAsync_HandlesNullDescription()
//        {
//            var roleId = 3;
//            var program = new LoyaltyProgram(Guid.NewGuid(), null);

//            await _repository.AddAsync(program, roleId);

//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedProgram = await context.LoyaltyPrograms.FirstOrDefaultAsync();
//            Assert.NotNull(addedProgram);
//            Assert.Null(addedProgram.Description);
//        }
//    }
//}