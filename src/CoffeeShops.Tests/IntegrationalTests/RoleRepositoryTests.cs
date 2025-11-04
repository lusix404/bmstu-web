//using CoffeeShops.DataAccess.Context;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Interfaces.Repositories;
//using CoffeeShops.Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using Xunit;


//namespace CoffeeShops.DataAccess.Tests
//{
//    public class RoleRepositoryTests
//    {
//        private IRoleRepository GetInMemoryRepository()
//        {
//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;

//            var context = new CoffeeShopsContext(options);
//            return new RoleRepository(context);
//        }

//        [Fact]
//        public async Task AddRoleAsync_ReturnCorrectRole()
//        {
//            var repository = GetInMemoryRepository();
//            var role = new Role(2, "moderator");

//            await repository.AddAsync(role);

//            var result = await repository.GetRoleByIdAsync(role!.Id_role);

//            Assert.NotNull(result);
//            Assert.Equal(role.Id_role, result.Id_role);
//            Assert.Equal(role.Name, result.Name);
//        }

//        [Fact]
//        public async Task RemoveRoleAsync_Success()
//        {
//            var repository = GetInMemoryRepository();
//            var role = new Role(2, "moderator");

//            await repository.AddAsync(role);

//            var f = (await repository.GetAllRolesAsync()).First();

//            await repository.RemoveAsync(f.Id_role);
//            var deleted = await repository.GetRoleByIdAsync(f.Id_role);

//            Assert.Null(deleted);

//        }

//    }
//}