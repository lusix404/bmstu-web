//using CoffeeShops.DataAccess.Context;
//using CoffeeShops.DataAccess.Converters;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using CoffeeShops.Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;

//namespace CoffeeShops.DataAccess.Tests
//{
//    public class CategoryRepositoryTests
//    {
//        private readonly Mock<IDbContextFactory> _mockContextFactory;
//        private readonly ICategoryRepository _repository;

//        public CategoryRepositoryTests()
//        {
//            _mockContextFactory = new Mock<IDbContextFactory>();

//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            var testContext = new CoffeeShopsContext(options);
//            _mockContextFactory.Setup(x => x.GetDbContext(It.IsAny<int>()))
//                .Returns(testContext);

//            _repository = new CategoryRepository(_mockContextFactory.Object);
//        }

//        [Fact]
//        public async Task GetCategoryByIdAsync_ReturnsCorrectCategory()
//        {

//            var roleId = 1;
//            var CategoryId = Guid.NewGuid();
//            var testCategory = new Category(
//                CategoryId,
//                Guid.NewGuid(),
//                "Test Category",
//                "https://test.com",
//                5);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            context.Companies.Add(CategoryConverter.ConvertToDbModel(testCategory));
//            await context.SaveChangesAsync();


//            var result = await _repository.GetCategoryByIdAsync(CategoryId, roleId);


//            Assert.NotNull(result);
//            Assert.Equal(CategoryId, result.Id_Category);
//            Assert.Equal("Test Category", result.Name);
//            Assert.Equal("https://test.com", result.Website);
//            Assert.Equal(5, result.AmountCoffeeShops);
//        }

//        [Fact]
//        public async Task GetCategoryByIdAsync_ReturnsNullForNonExistingCategory()
//        {

//            var nonExistingId = Guid.NewGuid();
//            var roleId = 1;


//            var result = await _repository.GetCategoryByIdAsync(nonExistingId, roleId);


//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task GetAllCompaniesAsync_ReturnsAllCompanies()
//        {

//            var roleId = 1;
//            var testCompanies = new[]
//            {
//                new Category(Guid.NewGuid(), Guid.NewGuid(), "Category 1", "https://Category1.com", 2),
//                new Category(Guid.NewGuid(), Guid.NewGuid(), "Category 2", "https://Category2.com", 5),
//                new Category(Guid.NewGuid(), Guid.NewGuid(), "Category 3", "https://Category3.com", 1)
//            };


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            foreach (var Category in testCompanies)
//            {
//                context.Companies.Add(CategoryConverter.ConvertToDbModel(Category));
//            }
//            await context.SaveChangesAsync();


//            var result = await _repository.GetAllCompaniesAsync(roleId);


//            Assert.NotNull(result);
//            Assert.Equal(3, result.Count);
//            Assert.Contains(result, c => c.Name == "Category 1");
//            Assert.Contains(result, c => c.Name == "Category 2");
//            Assert.Contains(result, c => c.Name == "Category 3");
//        }

//        [Fact]
//        public async Task AddAsync_AddsCategorySuccessfully()
//        {

//            var roleId = 3;
//            var Category = new Category(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                "New Category",
//                "https://new.com",
//                0);


//            await _repository.AddAsync(Category, roleId);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedCategory = await context.Companies.FirstOrDefaultAsync();
//            Assert.NotNull(addedCategory);
//            Assert.Equal(Category.Name, addedCategory.Name);
//            Assert.Equal(Category.Website, addedCategory.Website);
//            Assert.Equal(Category.AmountCoffeeShops, addedCategory.AmountCoffeeShops);
//        }

//        [Fact]
//        public async Task RemoveAsync_DeletesCategorySuccessfully()
//        {

//            var roleId = 3;
//            var CategoryId = Guid.NewGuid();
//            var testCategory = new Category(
//                CategoryId,
//                Guid.NewGuid(),
//                "To Delete",
//                "https://delete.com",
//                0);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            context.Companies.Add(CategoryConverter.ConvertToDbModel(testCategory));
//            await context.SaveChangesAsync();


//            await _repository.RemoveAsync(CategoryId, roleId);


//            var deletedCategory = await context.Companies.FindAsync(CategoryId);
//            Assert.Null(deletedCategory);
//        }

//        [Fact]
//        public async Task AddAsync_HandlesNullWebsite()
//        {

//            var roleId = 3;
//            var Category = new Category(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                "No Website Category",
//                null,
//                3);


//            await _repository.AddAsync(Category, roleId);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedCategory = await context.Companies.FirstOrDefaultAsync();
//            Assert.NotNull(addedCategory);
//            Assert.Null(addedCategory.Website);
//            Assert.Equal(3, addedCategory.AmountCoffeeShops);
//        }
//    }
//}





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

namespace CoffeeShops.DataAccess.Tests.Repositories
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly CoffeeShopsContext _context;
        private readonly ICategoryRepository _repository;
        private const string ConnectionString = "Host=localhost;Database=category_Category;Username=postgres;Password=lucy2004";

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            _context = new CoffeeShopsContext(options);


            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();


            SeedTestData();


            var mockFactory = new Mock<IDbContextFactory>();
            mockFactory.Setup(f => f.GetDbContext(It.IsAny<int>()))
                .Returns(_context);

            _repository = new CategoryRepository(mockFactory.Object);
        }

        private void SeedTestData()
        {
            if (!_context.Roles.Any())
            {
                _context.Roles.AddRange(
                    new RoleDb(1, "ordinary_user"),
                    new RoleDb(2, "moderator"),
                    new RoleDb(3, "administrator")
                );
                _context.SaveChanges();
            }
            if (!_context.LoyaltyPrograms.Any())
            {
                _context.LoyaltyPrograms.AddRange(
                    new LoyaltyProgramDb(Guid.NewGuid(), "lp1")
                );
                _context.SaveChanges();
            }
            if (!_context.Users.Any())
            {
                _context.Users.AddRange(
                    new UserDb(Guid.NewGuid(), 1, "user1", "pass1", new DateTime(1990, 1, 1), "user1@test.com"),
                    new UserDb(Guid.NewGuid(), 2, "moder1", "pass2", new DateTime(1995, 1, 1), "moder1@test.com"),
                    new UserDb(Guid.NewGuid(), 3, "admin1", "pass3", new DateTime(1985, 1, 1), "admin1@test.com")
                );
                _context.SaveChanges();
            }
            if (!_context.Categories.Any())
            {
                _context.Categories.AddRange(
                    new CategoryDb(Guid.NewGuid(),  "Category1"),
                    new CategoryDb(Guid.NewGuid(),  "Category2")
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
        public async Task GetCategoryByIdAsync_ShouldReturnCorrectUser()
        {

            var expectedUser = _context.Categories.First();


            var result = await _repository.GetCategoryByIdAsync(expectedUser.Id_category, 1);


            Assert.NotNull(result);
            Assert.Equal(expectedUser.Id_category, result.Id_category);
        }

      

    }
}