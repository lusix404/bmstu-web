using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Company;
using CoffeeShops.DTOs.Pagination;

namespace CoffeeShops.Services.Interfaces.Services
{
    public interface ICompanyService
    {
        public Task<Company?> GetCompanyByIdAsync(Guid company_id, int id_role);
        public Task<PaginatedResponse<Company>>? GetAllCompaniesAsync(CompanyFilters filters, int page, int limit, int id_role);
    }
}
