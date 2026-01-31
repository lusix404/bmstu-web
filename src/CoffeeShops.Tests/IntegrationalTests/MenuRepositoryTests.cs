//using CoffeeShops.DataAccess.Context;
//using CoffeeShops.DataAccess.Repositories;
//using CoffeeShops.Domain.Models;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Xunit;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using CoffeeShops.DataAccess.Models;
//using CoffeeShops.Domain.Interfaces.Repositories;

//namespace CoffeeShops.DataAccess.Tests
//{
//    public class MenuRepositoryTests
//    {
//        private readonly Mock<IDbContextFactory> _mockContextFactory;
//        private readonly IMenuRepository _repository;
//        private readonly CoffeeShopsContext _testContext;

//        public MenuRepositoryTests()
//        {
//            _mockContextFactory = new Mock<IDbContextFactory>();

//            var options = new DbContextOptionsBuilder<CoffeeShopsContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//                .Options;

//            _testContext = new CoffeeShopsContext(options);
//            InitializeTestData(_testContext);

//            _mockContextFactory.Setup(x => x.GetDbContext(It.IsAny<int>()))
//                .Returns(_testContext);

//            _repository = new MenuRepository(_mockContextFactory.Object);
//        }

//        private void InitializeTestData(CoffeeShopsContext context)
//        {
//            if (!context.Companies.Any())
//            {
//                context.Companies.AddRange(
//                    new CompanyDb(Guid.NewGuid(), Guid.NewGuid(), "Company 1", "a.ru", 5),
//                    new CompanyDb(Guid.NewGuid(), Guid.NewGuid(), "Company 2", "b.ru", 2)
//                );
//            }

//            if (!context.Drinks.Any())
//            {
//                context.Drinks.AddRange(
//                    new DrinkDb(Guid.NewGuid(), "Coffee"),
//                    new DrinkDb(Guid.NewGuid(), "Tea")
//                );
//            }
//            context.SaveChanges();
//        }

//        [Fact]
//        public async Task GetMenuByCompanyId_ReturnsCorrectMenuItems()
//        {
//            var roleId = 1;
//            var companyId = _testContext.Companies.First().Id_company;
//            var drinkId = _testContext.Drinks.First().Id_drink;
//            var menuItem = new Menu(drinkId, companyId, 250, 150.50m);

//            await _repository.AddAsync(menuItem, roleId);

//            var result = await _repository.GetMenuByCompanyId(companyId, roleId);

//            Assert.NotNull(result);
//            Assert.Single(result);
//            Assert.Equal(drinkId, result[0].Id_drink);
//            Assert.Equal(companyId, result[0].Id_company);
//        }

//        [Fact]
//        public async Task GetCompaniesByDrinkIdAsync_ReturnsCorrectCompanies()
//        {
//            var roleId = 1;
//            var company1 = _testContext.Companies.First();
//            var company2 = _testContext.Companies.Skip(1).First();
//            var drinkId = _testContext.Drinks.First().Id_drink;

//            await _repository.AddAsync(new Menu(drinkId, company1.Id_company, 250, 150.50m), roleId);
//            await _repository.AddAsync(new Menu(drinkId, company2.Id_company, 300, 180.00m), roleId);

//            var result = await _repository.GetCompaniesByDrinkIdAsync(drinkId, roleId);

//            Assert.NotNull(result);
//            Assert.Equal(2, result.Count);
//            Assert.Contains(result, c => c.Id_company == company1.Id_company);
//            Assert.Contains(result, c => c.Id_company == company2.Id_company);
//        }

//        [Fact]
//        public async Task AddAsync_AddsMenuItemSuccessfully()
//        {
//            var roleId = 3;
//            var companyId = _testContext.Companies.First().Id_company;
//            var drinkId = _testContext.Drinks.First().Id_drink;
//            var menuItem = new Menu(drinkId, companyId, 250, 150.50m);

//            await _repository.AddAsync(menuItem, roleId);

//            var result = await _repository.GetMenuByCompanyId(companyId, roleId);
//            Assert.NotNull(result);
//            Assert.Single(result);
//            Assert.Equal(menuItem.Id_drink, result[0].Id_drink);
//            Assert.Equal(menuItem.Id_company, result[0].Id_company);
//        }

//        [Fact]
//        public async Task RemoveAsync_RemovesMenuItemSuccessfully()
//        {
//            var roleId = 3;
//            var companyId = _testContext.Companies.First().Id_company;
//            var drinkId = _testContext.Drinks.First().Id_drink;
//            var menuItem = new Menu(drinkId, companyId, 250, 150.50m);

//            await _repository.AddAsync(menuItem, roleId);
//            await _repository.RemoveAsync(drinkId, companyId, roleId);

//            var result = await _repository.GetMenuByCompanyId(companyId, roleId);
//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task RemoveRecordAsync_RemovesMenuRecordSuccessfully()
//        {
//            var roleId = 3;
//            var companyId = _testContext.Companies.First().Id_company;
//            var drinkId = _testContext.Drinks.First().Id_drink;
//            var menuItem = new Menu(drinkId, companyId, 250, 150.50m);

//            await _repository.AddAsync(menuItem, roleId);
//            var addedItem = (await _repository.GetMenuByCompanyId(companyId, roleId)).First();

//            await _repository.RemoveRecordAsync(addedItem.Id_menu, roleId);

//            var result = await _repository.GetMenuByCompanyId(companyId, roleId);
//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task GetMenuByCompanyId_ReturnsEmptyListForNoMenuItems()
//        {
//            var roleId = 1;
//            var companyId = Guid.NewGuid(); // New company with no menu items

//            var result = await _repository.GetMenuByCompanyId(companyId, roleId);

//            Assert.NotNull(result);
//            Assert.Empty(result);
//        }
//    }
//}