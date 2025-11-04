using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Services;

public class CoffeeShopService : ICoffeeShopService
{
    private readonly ICoffeeShopRepository _coffeeshopRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<CoffeeShopService> _logger;

    public CoffeeShopService(
        ICoffeeShopRepository coffeeshopRepository,
        ICompanyRepository companyRepository,
        ILogger<CoffeeShopService> logger)
    {
        _coffeeshopRepository = coffeeshopRepository;
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role)
    {
        _logger.LogInformation("Getting coffee shop by ID: {CoffeeShopId}",
            coffeeshop_id, id_role);

        try
        {
            var coffeeshop = await _coffeeshopRepository.GetCoffeeShopByIdAsync(coffeeshop_id, id_role);

            if (coffeeshop == null)
            {
                _logger.LogWarning("Coffee shop not found: {CoffeeShopId}", coffeeshop_id);
                throw new CoffeeShopNotFoundException($"Coffeeshop with id={coffeeshop_id} was not found");
            }

            _logger.LogInformation("Successfully retrieved coffee shop: {CoffeeShopId}", coffeeshop_id);
            return coffeeshop;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coffee shop by ID: {CoffeeShopId}", coffeeshop_id);
            throw;
        }
    }

    public async Task<PaginatedResponse<CoffeeShop>>? GetCoffeeShopsByCompanyIdAsync(Guid company_id, CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        _logger.LogInformation("Getting coffee shops for company ID: {CompanyId}",
            company_id, id_role);

        try
        {
            (var coffeeshops, int total) = await _coffeeshopRepository.GetCoffeeShopsByCompanyIdAsync(company_id, filters, page, limit, id_role);

            if (coffeeshops == null || !coffeeshops.Any())
            {
                _logger.LogWarning("No coffee shops found for company: {CompanyId}", company_id);
                throw new CoffeeShopsForCompanyNotFoundException($"No coffeeshops for company with id={company_id} was found");
            }

            _logger.LogInformation("Successfully retrieved {Count} coffee shops for company: {CompanyId}",
                coffeeshops.Count, company_id);
            return new PaginatedResponse<CoffeeShop> { Data = coffeeshops, Total = total, Limit = limit, Page = page };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coffee shops for company ID: {CompanyId}", company_id);
            throw;
        }
    }
    public async Task<PaginatedResponse<CoffeeShop>>? GetAllCoffeeShopsAsync(CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        try
        {
            (var coffeeshops, int total) = await _coffeeshopRepository.GetAllCoffeeShopsAsync(filters, page, limit, id_role);

            if (coffeeshops == null || (coffeeshops.Count == 0))
            {
                _logger.LogWarning("There are no coffeeshops in system");
                throw new CoffeeShopsForCompanyNotFoundException($"There are no coffeeshops in system");
            }

            _logger.LogInformation("Successfully retrieved all coffee shops");
            return new PaginatedResponse<CoffeeShop> { Data = coffeeshops, Total = total, Limit = limit, Page = page };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all coffee shops");
            throw;
        }
    }

    public async Task<Guid> AddCoffeeShopAsync(CoffeeShop coffeeshop, int id_role)
    {
        _logger.LogInformation("Adding new coffee shop for company: {CompanyId}",
            coffeeshop?.Id_company);

        try
        {
            if (coffeeshop == null)
            {
                _logger.LogError("Attempt to add null coffee shop");
                throw new ArgumentNullException(nameof(coffeeshop));
            }

            if (string.IsNullOrWhiteSpace(coffeeshop.Address))
            {
                _logger.LogError("Attempt to add coffee shop with empty address");
                throw new CoffeeShopIncorrectAtributeException("Coffeeshop's address cannot be empty");
            }

            var company = await _companyRepository.GetCompanyByIdAsync(coffeeshop.Id_company, id_role);
            if (company == null)
            {
                _logger.LogError("Company not found: {CompanyId}", coffeeshop.Id_company);
                throw new CompanyNotFoundException($"Company with id={coffeeshop.Id_company} was not found");
            }

            Guid id = await _coffeeshopRepository.AddAsync(coffeeshop, id_role);
            _logger.LogInformation("Successfully added coffee shop");
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding coffee shop for company: {CompanyId}",
                coffeeshop?.Id_company);
            throw;
        }
    }

    public async Task DeleteCoffeeShopAsync(Guid coffeeshop_id, int id_role)
    {
        _logger.LogInformation("Deleting coffee shop: {CoffeeShopId}", coffeeshop_id);

        try
        {
            var coffeeshop = await _coffeeshopRepository.GetCoffeeShopByIdAsync(coffeeshop_id, id_role);
            if (coffeeshop == null)
            {
                _logger.LogWarning("Coffee shop not found for deletion: {CoffeeShopId}", coffeeshop_id);
                throw new CoffeeShopNotFoundException($"Coffeeshop with id={coffeeshop_id} was not found");
            }

            await _coffeeshopRepository.RemoveAsync(coffeeshop_id, id_role);
            _logger.LogInformation("Successfully deleted coffee shop: {CoffeeShopId}", coffeeshop_id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coffee shop: {CoffeeShopId}", coffeeshop_id);
            throw;
        }
    }
}