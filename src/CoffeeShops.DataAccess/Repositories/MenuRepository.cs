using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using Npgsql;
using CoffeeShops.DTOs.User;

namespace CoffeeShops.DataAccess.Repositories;

public class MenuRepository : IMenuRepository
{
    private IDbContextFactory _contextFactory;
    public MenuRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<(List<Menu>? data, int total)> GetMenuByCompanyId(Guid company_id, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        //var menuItems = await _context.Menus
        //    .Where(m => m.Id_company == company_id)
        //    .AsNoTracking()
        //    .ToListAsync();

        //return menuItems?.ConvertAll(MenuConverter.ConvertToDomainModel);

        var menuItems = await _context.Menus
            .Where(m => m.Id_company == company_id)
            .Join(_context.Drinks,
                    m => m.Id_drink,
                    dr => dr.Id_drink,
                    (m, dr) => new Menu (m.Id_drink, m.Id_company, m.Size, m.Price, dr.Name))
            .OrderBy(x => x.DrinkName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return (menuItems, menuItems.Count);
    }

    public async Task<(List<Company>? data, int total)> GetCompaniesByDrinkIdAsync(Guid drink_id, int page, int limit, int id_role)
    {

        var _context = _contextFactory.GetDbContext(id_role);
        var companies = await _context.Menus
            .Where(m => m.Id_drink == drink_id)
            .Join(_context.Companies,
                m => m.Id_company,
                c => c.Id_company,
                (m, c) => c)
            .OrderBy(x => x.Name)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return (companies?.ConvertAll(CompanyConverter.ConvertToDomainModel), companies.Count);
    }

    public async Task AddAsync(Menu menuRecord, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var dbMenu = MenuConverter.ConvertToDbModel(menuRecord);
        await _context.Menus.AddAsync(dbMenu);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid drink_id, Guid company_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var menuItem = await _context.Menus
            .FirstOrDefaultAsync(m => m.Id_drink == drink_id && m.Id_company == company_id);

        if (menuItem != null)
        {
            _context.Menus.Remove(menuItem);
            await _context.SaveChangesAsync();
        }
    }
}