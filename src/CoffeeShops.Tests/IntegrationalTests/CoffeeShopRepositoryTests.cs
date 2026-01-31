

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
    public class CoffeeShopRepositoryTests : IDisposable
    {
        private readonly CoffeeShopsContext _context;
        private readonly ICoffeeShopRepository _repository;
        private const string ConnectionString = "Host=localhost;Database=CoffeeShop_CoffeeShop;Username=postgres;Password=lucy2004";

        public CoffeeShopRepositoryTests()
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

            _repository = new CoffeeShopRepository(mockFactory.Object);
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

            if (!_context.CoffeeShops.Any())
            {
                var lp_d = _context.Companies.First().Id_company;
                _context.CoffeeShops.AddRange(
                    new CoffeeShopDb(Guid.NewGuid(), lp_d, "cs1", "2"),
                    new CoffeeShopDb(Guid.NewGuid(), lp_d, "cs2", "1")
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
        public async Task GetCoffeeShopByIdAsync_ShouldReturnCorrectUser()
        {

            var expectedUser = _context.CoffeeShops.First();


            var result = await _repository.GetCoffeeShopByIdAsync(expectedUser.Id_coffeeshop, 1);


            Assert.NotNull(result);
            Assert.Equal(expectedUser.Id_coffeeshop, result.Id_coffeeshop);
        }

      

    }
}