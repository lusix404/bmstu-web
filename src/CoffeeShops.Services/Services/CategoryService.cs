using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.User;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid category_id, int id_role)
    {
        _logger.LogInformation("Getting category by ID: {CategoryId}", category_id, id_role);
        
        try
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(category_id, id_role);
            
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", category_id);
                throw new CategoryNotFoundException($"Category with id={category_id} was not found");
            }

            _logger.LogInformation("Successfully retrieved category: {CategoryId}", category_id);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by ID: {CategoryId}", category_id);
            throw;
        }
    }

    //возвр. domain.PaginatedResponse[domain.Category]
    public async Task<PaginatedResponse<Category>>? GetAllCategoriesAsync(int page, int limit, int id_role)
    {
        _logger.LogInformation("Getting all categories");
        try
        {
            (List<Category> categories, int total) = await _categoryRepository.GetAllCategoriesAsync(page, limit, id_role);

            if (categories == null || !categories.Any())
            {
                _logger.LogWarning("No categories found in database");
                throw new CategoryNotFoundException("There are no categories in the database");
            }

            _logger.LogInformation("Successfully retrieved {CategoryCount} categories", categories.Count);
            return new PaginatedResponse<Category> { Data = categories, Total = total, Limit = limit, Page = page };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            throw;
        }
    }

    public async Task<Guid> AddCategoryAsync(Category category, int id_role)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        if (string.IsNullOrWhiteSpace(category.Name))
            throw new CategoryIncorrectAtributeException("Имя категории не должно быть пустым");

        try
        {
            Guid id = await _categoryRepository.AddCategoryAsync(category, id_role);
            return id;
        }
        catch (CategoryUniqueException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}