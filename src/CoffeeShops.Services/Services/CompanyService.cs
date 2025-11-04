using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.Company;

namespace CoffeeShops.Services.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(
            ICompanyRepository companyRepository,
            ILogger<CompanyService> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<Company?> GetCompanyByIdAsync(Guid company_id, int id_role)
        {
            _logger.LogInformation("Getting company by ID: {CompanyId}", company_id);

            try
            {
                var company = await _companyRepository.GetCompanyByIdAsync(company_id, id_role);

                if (company == null)
                {
                    _logger.LogWarning("Company not found: {CompanyId}", company_id);
                    throw new CompanyNotFoundException($"Company with id={company_id} was not found");
                }

                _logger.LogInformation("Successfully retrieved company: {CompanyName} (ID: {CompanyId})",
                    company.Name, company_id);
                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company by ID: {CompanyId}", company_id);
                throw;
            }
        }

        public async Task<PaginatedResponse<Company>>? GetAllCompaniesAsync(CompanyFilters filters, int page, int limit, int id_role)
        {
            _logger.LogInformation("Getting all companies", id_role);

            try
            {
                (var companies, int total) = await _companyRepository.GetAllCompaniesAsync(filters, page, limit, id_role);

                if (companies == null || !companies.Any())
                {
                    _logger.LogWarning("No companies found in database");
                    throw new NoCompaniesFoundException("There is no companies in data base");
                }

                _logger.LogInformation("Successfully retrieved {CompanyCount} companies", companies.Count);
                return new PaginatedResponse<Company> { Data = companies, Total = total, Limit = limit, Page = page };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all companies");
                throw;
            }
        }

        public async Task<Guid> AddCompanyAsync(Company company, int id_role)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            if (string.IsNullOrWhiteSpace(company.Name))
                throw new CompanyIncorrectAtributeException("Название сети кофеен не может быть пустым");

            if (!company.Validate())
                throw new CompanyIncorrectAtributeException("Некорректное значение веб-сайт или количества кофеен");


            Guid id = await _companyRepository.AddAsync(company, id_role);
            return id;
        }

        public async Task DeleteCompanyAsync(Guid companyId, int id_role)
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId, id_role);
            if (company == null)
            {
                throw new CompanyNotFoundException($"Сеть кофеен с id_company={companyId} не найдена");
            }

            await _companyRepository.RemoveAsync(companyId, id_role);
        }
    }
}