using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using Npgsql;
using CoffeeShops.DTOs.Company;

namespace CoffeeShops.DataAccess.Repositories;
public class CompanyRepository : ICompanyRepository
{

    private IDbContextFactory _contextFactory;
    public CompanyRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Company?> GetCompanyByIdAsync(Guid company_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var company = await _context.Companies.FindAsync(company_id);
        return company != null ? CompanyConverter.ConvertToDomainModel(company) : null;
    }

    public async Task<(List<Company>? data, int total)> GetAllCompaniesAsync(CompanyFilters filters, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        //var companys = await _context.Companies
        //    .AsNoTracking()
        //    .ToListAsync();

        //return companys.ConvertAll(d => CompanyConverter.ConvertToDomainModel(d));
        var query = _context.Companies.AsQueryable();
        if (filters.Id_drink != null)
        {
            query = query.Where(c => _context.Menus.Any(m => m.Id_company == c.Id_company
            && m.Id_drink == filters.Id_drink));
        }

        var total = await query.CountAsync(); //количество элементов, удовлетворяющих условиям

        var companies = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (companies.ConvertAll(c => CompanyConverter.ConvertToDomainModel(c)), total);
    }

    public async Task<Guid> AddAsync(Company company, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var companyDb = CompanyConverter.ConvertToDbModel(company);
        await _context.Companies.AddAsync(companyDb);
        await _context.SaveChangesAsync();
        return companyDb.Id_company;
    }

    public async Task RemoveAsync(Guid company_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var company = await _context.Companies.FindAsync(company_id);
        if (company != null)
        {
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }
    }
}