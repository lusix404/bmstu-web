

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
    public class UserRepositoryTests : IDisposable
    {
        private readonly CoffeeShopsContext _context;
        private readonly IUserRepository _repository;
        private const string ConnectionString = "Host=localhost;Database=test_coffeeshops;Username=postgres;Password=lucy2004";

        public UserRepositoryTests()
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

            _repository = new UserRepository(mockFactory.Object);
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
            if (!_context.Users.Any())
            {
                _context.Users.AddRange(
                    new UserDb(Guid.NewGuid(), 1, "user1", "pass1", new DateTime(1990, 1, 1), "user1@test.com"),
                    new UserDb(Guid.NewGuid(), 2, "moder1", "pass2", new DateTime(1995, 1, 1), "moder1@test.com"),
                    new UserDb(Guid.NewGuid(), 3, "admin1", "pass3", new DateTime(1985, 1, 1), "admin1@test.com")
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
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
        {
            
            var expectedUser = _context.Users.First();

            
            var result = await _repository.GetUserByIdAsync(expectedUser.Id_user, 1);

           
            Assert.NotNull(result);
            Assert.Equal(expectedUser.Id_user, result.Id_user);
            Assert.Equal(expectedUser.Login, result.Login);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNullForNonExistingUser()
        {
            
            var result = await _repository.GetUserByIdAsync(Guid.NewGuid(), 1);

           
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByLoginAsync_ShouldReturnCorrectUser()
        {
            
            var expectedUser = _context.Users.First();

            
            var result = await _repository.GetUserByLoginAsync(expectedUser.Login, 1);

           
            Assert.NotNull(result);
            Assert.Equal(expectedUser.Login, result.Login);
        }

        

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            
            var expectedCount = _context.Users.Count();

            
            var result = await _repository.GetAllUsersAsync(1);

           
            Assert.Equal(expectedCount, result.Count);
            Assert.All(result, user => Assert.NotNull(user.Login));
        }
    }
}