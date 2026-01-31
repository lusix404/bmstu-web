using CoffeeShops.Domain.Models;
using CoffeeShops.DTOs.Company;

namespace CoffeeShops.Domain.Interfaces.Repositories
{
    public interface ICompanyRepository
    {
        Task<Company?> GetCompanyByIdAsync(Guid company_id, int id_role);
        Task<(List<Company>? data, int total)> GetAllCompaniesAsync(CompanyFilters filters, int page, int limit, int id_role);

        Task<Guid> AddAsync(Company company, int id_role);
        Task RemoveAsync(Guid company_id, int id_role);
        Task RemoveCompanyWithAllDataAsync(Guid company_id, int id_role);

    }
}
