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
//    public class CompanyRepositoryTests
//    {
//        private readonly Mock<IDbContextFactory> _mockContextFactory;
//        private readonly ICompanyRepository _repository;

//        public CompanyRepositoryTests()
//        {
//            _mockContextFactory = new Mock<IDbContextFactory>();

//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            var testContext = new CoffeeShopsContext(options);
//            _mockContextFactory.Setup(x => x.GetDbContext(It.IsAny<int>()))
//                .Returns(testContext);

//            _repository = new CompanyRepository(_mockContextFactory.Object);
//        }

//        [Fact]
//        public async Task GetCompanyByIdAsync_ReturnsCorrectCompany()
//        {

//            var roleId = 1;
//            var companyId = Guid.NewGuid();
//            var testCompany = new Company(
//                companyId,
//                Guid.NewGuid(),
//                "Test Company",
//                "https://test.com",
//                5);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            context.Companies.Add(CompanyConverter.ConvertToDbModel(testCompany));
//            await context.SaveChangesAsync();


//            var result = await _repository.GetCompanyByIdAsync(companyId, roleId);


//            Assert.NotNull(result);
//            Assert.Equal(companyId, result.Id_company);
//            Assert.Equal("Test Company", result.Name);
//            Assert.Equal("https://test.com", result.Website);
//            Assert.Equal(5, result.AmountCoffeeShops);
//        }

//        [Fact]
//        public async Task GetCompanyByIdAsync_ReturnsNullForNonExistingCompany()
//        {

//            var nonExistingId = Guid.NewGuid();
//            var roleId = 1;


//            var result = await _repository.GetCompanyByIdAsync(nonExistingId, roleId);


//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task GetAllCompaniesAsync_ReturnsAllCompanies()
//        {

//            var roleId = 1;
//            var testCompanies = new[]
//            {
//                new Company(Guid.NewGuid(), Guid.NewGuid(), "Company 1", "https://company1.com", 2),
//                new Company(Guid.NewGuid(), Guid.NewGuid(), "Company 2", "https://company2.com", 5),
//                new Company(Guid.NewGuid(), Guid.NewGuid(), "Company 3", "https://company3.com", 1)
//            };


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            foreach (var company in testCompanies)
//            {
//                context.Companies.Add(CompanyConverter.ConvertToDbModel(company));
//            }
//            await context.SaveChangesAsync();


//            var result = await _repository.GetAllCompaniesAsync(roleId);


//            Assert.NotNull(result);
//            Assert.Equal(3, result.Count);
//            Assert.Contains(result, c => c.Name == "Company 1");
//            Assert.Contains(result, c => c.Name == "Company 2");
//            Assert.Contains(result, c => c.Name == "Company 3");
//        }

//        [Fact]
//        public async Task AddAsync_AddsCompanySuccessfully()
//        {

//            var roleId = 3;
//            var company = new Company(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                "New Company",
//                "https://new.com",
//                0);


//            await _repository.AddAsync(company, roleId);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedCompany = await context.Companies.FirstOrDefaultAsync();
//            Assert.NotNull(addedCompany);
//            Assert.Equal(company.Name, addedCompany.Name);
//            Assert.Equal(company.Website, addedCompany.Website);
//            Assert.Equal(company.AmountCoffeeShops, addedCompany.AmountCoffeeShops);
//        }

//        [Fact]
//        public async Task RemoveAsync_DeletesCompanySuccessfully()
//        {

//            var roleId = 3;
//            var companyId = Guid.NewGuid();
//            var testCompany = new Company(
//                companyId,
//                Guid.NewGuid(),
//                "To Delete",
//                "https://delete.com",
//                0);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            context.Companies.Add(CompanyConverter.ConvertToDbModel(testCompany));
//            await context.SaveChangesAsync();


//            await _repository.RemoveAsync(companyId, roleId);


//            var deletedCompany = await context.Companies.FindAsync(companyId);
//            Assert.Null(deletedCompany);
//        }

//        [Fact]
//        public async Task AddAsync_HandlesNullWebsite()
//        {

//            var roleId = 3;
//            var company = new Company(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                "No Website Company",
//                null,
//                3);


//            await _repository.AddAsync(company, roleId);


//            var context = _mockContextFactory.Object.GetDbContext(roleId);
//            var addedCompany = await context.Companies.FirstOrDefaultAsync();
//            Assert.NotNull(addedCompany);
//            Assert.Null(addedCompany.Website);
//            Assert.Equal(3, addedCompany.AmountCoffeeShops);
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
    public class CompanyRepositoryTests : IDisposable
    {
        private readonly CoffeeShopsContext _context;
        private readonly ICompanyRepository _repository;
        private const string ConnectionString = "Host=localhost;Database=test_company;Username=postgres;Password=lucy2004";

        public CompanyRepositoryTests()
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

            _repository = new CompanyRepository(mockFactory.Object);
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
            if (!_context.Companies.Any())
            {
                var lp_d = _context.LoyaltyPrograms.First().Id_lp;
                _context.Companies.AddRange(
                    new CompanyDb(Guid.NewGuid(), lp_d, "company1", "web1", 0),
                    new CompanyDb(Guid.NewGuid(), lp_d, "company2", "web2", 0)
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
        public async Task GetCompanyByIdAsync_ShouldReturnCorrectUser()
        {

            var expectedUser = _context.Companies.First();


            var result = await _repository.GetCompanyByIdAsync(expectedUser.Id_company, 1);


            Assert.NotNull(result);
            Assert.Equal(expectedUser.Id_company, result.Id_company);
        }

        [Fact]
        public async Task GetCompanyByIdAsync_ShouldReturnNullForNonExistingUser()
        {

            var result = await _repository.GetCompanyByIdAsync(Guid.NewGuid(), 1);


            Assert.Null(result);
        }


        
    }
}