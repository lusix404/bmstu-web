using Moq;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Services;
using Xunit;
using Microsoft.Extensions.Logging;

namespace CoffeeShops.Domain.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ILogger<CategoryService>> _mockLogger;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockLogger = new Mock<ILogger<CategoryService>>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_Success()
        {
            var categoryId = Guid.NewGuid();
            var expectedCategory = new Category(categoryId, "Test Category");

            _mockCategoryRepository.Setup(x => x.GetCategoryByIdAsync(categoryId, 3))
                .ReturnsAsync(expectedCategory);

            var result = await _categoryService.GetCategoryByIdAsync(categoryId, 3);

            Assert.Equal(expectedCategory, result);
            Assert.Equal(categoryId, result.Id_category);
            _mockCategoryRepository.Verify(x => x.GetCategoryByIdAsync(categoryId, 3), Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_CategoryNotFound()
        {
            var invalidId = Guid.NewGuid();

            _mockCategoryRepository.Setup(x => x.GetCategoryByIdAsync(invalidId, 3))
                .ReturnsAsync((Category?)null);

            var exception = await Assert.ThrowsAsync<CategoryNotFoundException>(
                () => _categoryService.GetCategoryByIdAsync(invalidId, 3));

            Assert.Equal($"Category with id={invalidId} was not found", exception.Message);
            _mockCategoryRepository.Verify(x => x.GetCategoryByIdAsync(invalidId, 3), Times.Once);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_Success()
        {
            var expectedCategories = new List<Category>
            {
                new Category(Guid.NewGuid(), "Category 1"),
                new Category(Guid.NewGuid(), "Category 2")
            };

            _mockCategoryRepository.Setup(x => x.GetAllCategoriesAsync(3))
                .ReturnsAsync(expectedCategories);

            var result = await _categoryService.GetAllCategoriesAsync(3);

            Assert.Equal(expectedCategories, result);
            Assert.Equal(2, result.Count);
            _mockCategoryRepository.Verify(x => x.GetAllCategoriesAsync(3), Times.Once);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_NoCategoriesExist()
        {
            _mockCategoryRepository.Setup(x => x.GetAllCategoriesAsync(3))
                .ReturnsAsync((List<Category>?)null);

            // Настроим логгер, чтобы он не падал на форматировании
            _mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            var exception = await Assert.ThrowsAsync<CategoryNotFoundException>(
                () => _categoryService.GetAllCategoriesAsync(3));

            Assert.Equal("There are no categories in the database", exception.Message);
            _mockCategoryRepository.Verify(x => x.GetAllCategoriesAsync(3), Times.Once);
        }


    }
}
