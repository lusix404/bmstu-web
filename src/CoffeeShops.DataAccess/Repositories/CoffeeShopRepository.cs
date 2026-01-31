using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using CoffeeShops.DTOs.CoffeeShop;

namespace CoffeeShops.DataAccess.Repositories;
public class CoffeeShopRepository : ICoffeeShopRepository
{
    private IDbContextFactory _contextFactory;
    public CoffeeShopRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    //public async Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role)
    //{
    //    var _context = _contextFactory.GetDbContext(id_role);
    //    var dbCoffeeShop = await _context.CoffeeShops
    //        .Join(_context.Companies,
    //              cs => cs.Id_company,
    //              comp => comp.Id_company,
    //              (cs, comp) => new CoffeeShop(cs.Id_coffeeshop,
    //                  cs.Id_company,
    //                  cs.Address,
    //                  cs.WorkingHours,
    //                  comp.Name))
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(cs => cs.Id_coffeeshop == coffeeshop_id);

    //    //return dbCoffeeShop != null ? CoffeeShopConverter.ConvertToDomainModel(dbCoffeeShop) : null;
    //    return dbCoffeeShop;
    public async Task<CoffeeShop?> GetCoffeeShopByIdAsync(Guid coffeeshop_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        var result = await _context.CoffeeShops
            .Where(cs => cs.Id_coffeeshop == coffeeshop_id)
            .Join(_context.Companies,
                  cs => cs.Id_company,
                  comp => comp.Id_company,
                  (cs, comp) => new { CoffeeShopDb = cs, CompanyName = comp.Name })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null)
            return null;

        return new CoffeeShop(
            result.CoffeeShopDb.Id_coffeeshop,
            result.CoffeeShopDb.Id_company,
            result.CoffeeShopDb.Address,
            result.CoffeeShopDb.WorkingHours,
            result.CompanyName
        );
    }

    //public async Task<(List<CoffeeShop>? data, int total)> GetAllCoffeeShopsAsync(CoffeeShopFilters filters, int page, int limit, int id_role)
    //{
    //    var _context = _contextFactory.GetDbContext(id_role);
    //    var query = _context.CoffeeShops.AsQueryable();
    //    if (filters.Id_company != null)
    //    {
    //        query = query.Where(u => u.Id_company == filters.Id_company);
    //    }
    //    else if (filters.OnlyFavorites && (filters.Id_user != null))
    //    {
    //        query = query.Where(cs => _context.FavCoffeeShops.Any(fav => fav.Id_coffeeshop == cs.Id_coffeeshop 
    //        && fav.Id_user == filters.Id_user));
    //    }

    //    var total = await query.CountAsync();

    //    //var coffeeshops = await query
    //    //    .OrderBy(cs => cs.Id_coffeeshop)
    //    //    .Skip((page - 1) * limit)
    //    //    .Take(limit)
    //    //    .ToListAsync();
    //    var coffeeshops = await query
    //        .Join(_context.Companies,
    //              cs => cs.Id_company,
    //              comp => comp.Id_company,
    //              (cs, comp) => new CoffeeShop(cs.Id_coffeeshop,
    //                  cs.Id_company,
    //                  cs.Address,
    //                  cs.WorkingHours,
    //                  comp.Name))
    //        .OrderBy(x => x.CompanyName)
    //        .Skip((page - 1) * limit)
    //        .Take(limit)
    //        .AsNoTracking()
    //        .ToListAsync();

    //    //return (coffeeshops.ConvertAll(cs => CoffeeShopConverter.ConvertToDomainModel(cs)), total);
    //    return (coffeeshops, total);
    public async Task<(List<CoffeeShop>? data, int total)> GetAllCoffeeShopsAsync(CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var query = _context.CoffeeShops.AsQueryable();

        if (filters.Id_company != null)
        {
            query = query.Where(u => u.Id_company == filters.Id_company);
        }
        if (filters.OnlyFavorites && (filters.Id_user != null))
        {
            query = query.Where(cs => _context.FavCoffeeShops.Any(fav => fav.Id_coffeeshop == cs.Id_coffeeshop
            && fav.Id_user == filters.Id_user));
        }

        var total = await query.CountAsync();

        var coffeeshops = await query
            .Join(_context.Companies,
                  cs => cs.Id_company,
                  comp => comp.Id_company,
                  (cs, comp) => new { CoffeeShopDb = cs, CompanyName = comp.Name })
            .OrderBy(x => x.CompanyName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        var result = coffeeshops.Select(x => new CoffeeShop(
            x.CoffeeShopDb.Id_coffeeshop,
            x.CoffeeShopDb.Id_company,
            x.CoffeeShopDb.Address,
            x.CoffeeShopDb.WorkingHours,
            x.CompanyName
        )).ToList();

        return (result, total);
    }

    public async Task<(List<CoffeeShop>? data, int total)> GetCoffeeShopsByCompanyIdAsync(Guid company_id, CoffeeShopFilters filters, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        //var dbCoffeeShops = await _context.CoffeeShops
        //    .Where(cs => cs.Id_company == company_id)
        //    .AsNoTracking()
        //    .ToListAsync();

        //return dbCoffeeShops?.ConvertAll(CoffeeShopConverter.ConvertToDomainModel);
        var query = _context.CoffeeShops.AsQueryable();
        if (filters.OnlyFavorites && (filters.Id_user != null))
        {
            query = query.Where(cs => _context.FavCoffeeShops.Any(fav => fav.Id_coffeeshop == cs.Id_coffeeshop
            && fav.Id_user == filters.Id_user));
        }
        var coffeeshops = await query
           .Where(cs => cs.Id_company == company_id)
           .OrderBy(cs => cs.Id_coffeeshop)
           .Skip((page - 1) * limit)
           .Take(limit)
           .AsNoTracking()
           .ToListAsync();
        return (coffeeshops.ConvertAll(cs => CoffeeShopConverter.ConvertToDomainModel(cs)), coffeeshops.Count);
    }

    public async Task<Guid> AddAsync(CoffeeShop coffeeshop, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var dbCoffeeShop = CoffeeShopConverter.ConvertToDbModel(coffeeshop);
        await _context.CoffeeShops.AddAsync(dbCoffeeShop);
        await _context.SaveChangesAsync();
        return dbCoffeeShop.Id_coffeeshop;
    }

    public async Task RemoveAsync(Guid coffeeshop_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var dbCoffeeShop = await _context.CoffeeShops
            .FirstOrDefaultAsync(cs => cs.Id_coffeeshop == coffeeshop_id);

        if (dbCoffeeShop != null)
        {
            _context.CoffeeShops.Remove(dbCoffeeShop);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveAllByCompanyIdAsync(Guid company_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var favs = await _context.FavCoffeeShops
                .Where(fav => _context.CoffeeShops
                    .Where(cs => cs.Id_company == company_id)
                    .Select(cs => cs.Id_coffeeshop)
                    .Contains(fav.Id_coffeeshop))
                .ToListAsync();

            if (favs.Any())
            {
                _context.FavCoffeeShops.RemoveRange(favs);
            }

            var coffeeShopsToDelete = await _context.CoffeeShops
                .Where(cs => cs.Id_company == company_id)
                .ToListAsync();

            if (coffeeShopsToDelete.Any())
            {
                _context.CoffeeShops.RemoveRange(coffeeShopsToDelete);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}