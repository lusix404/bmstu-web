using CoffeeShops.DataAccess.Converters;
using CoffeeShops.Domain.Interfaces.Repositories;
using CoffeeShops.Domain.Models;
using CoffeeShops.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using CoffeeShops.DataAccess.Models;
using Npgsql;
using CoffeeShops.DTOs.CoffeeShop;

namespace CoffeeShops.DataAccess.Repositories;
public class FavCoffeeShopsRepository : IFavCoffeeShopsRepository
{
    private IDbContextFactory _contextFactory;
    public FavCoffeeShopsRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<FavCoffeeShops>? GetRecordAsync(Guid coffeeshop_id, Guid user_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var record = await _context.FavCoffeeShops
    .FirstOrDefaultAsync(dc => dc.Id_coffeeshop == coffeeshop_id && dc.Id_user == user_id);

        return record != null ? FavCoffeeShopsConverter.ConvertToDomainModel(record) : null; ;
    }

    public async Task AddAsync(FavCoffeeShops added_coffeeshop, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var coffeeshopDb = FavCoffeeShopsConverter.ConvertToDbModel(added_coffeeshop);
        await _context.FavCoffeeShops.AddAsync(coffeeshopDb);
        await _context.SaveChangesAsync();

    }

    public async Task RemoveAsync(Guid user_id, Guid coffeeshop_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var favCoffeeShop = await _context.FavCoffeeShops
            .FirstOrDefaultAsync(fd => fd.Id_user == user_id && fd.Id_coffeeshop == coffeeshop_id);

        if (favCoffeeShop != null)
        {
            _context.FavCoffeeShops.Remove(favCoffeeShop);
            await _context.SaveChangesAsync();
        }

    }

    //public async Task<List<FavCoffeeShops>> GetAllFavCoffeeShopsAsync(Guid user_id, int id_role)
    //{
    //    var _context = _contextFactory.GetDbContext(id_role);
    //    var favCoffeeShops = await _context.FavCoffeeShops
    //        .Where(fd => fd.Id_user == user_id)
    //        .AsNoTracking()
    //        .ToListAsync();

    //    return favCoffeeShops.ConvertAll(d => FavCoffeeShopsConverter.ConvertToDomainModel(d));
    //}

    public async Task<(List<CoffeeShop>? data, int total)> GetAllFavCoffeeShopsAsync(Guid user_id, int page, int limit, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var query = _context.CoffeeShops.AsQueryable();
        query = query.Where(cs => _context.FavCoffeeShops.Any(fav => fav.Id_coffeeshop == cs.Id_coffeeshop
            && fav.Id_user == user_id));
        var total = await query.CountAsync();

        var coffeeshops = await query
            .Join(_context.Companies,
                  cs => cs.Id_company,
                  comp => comp.Id_company,
                  (cs, comp) => new CoffeeShop(cs.Id_coffeeshop,
                      cs.Id_company,
                      cs.Address,
                      cs.WorkingHours,
                      comp.Name))
            .OrderBy(x => x.CompanyName)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return (coffeeshops, total);
    }

    public async Task RemoveByCoffeeShopIdAsync(Guid coffeeshop_id, int id_role)
    {
        var _context = _contextFactory.GetDbContext(id_role);
        var favs = await _context.FavCoffeeShops.Where(dc => dc.Id_coffeeshop == coffeeshop_id)
            .ToListAsync();

        if (favs.Any())
        {
            _context.FavCoffeeShops.RemoveRange(favs);
            await _context.SaveChangesAsync();
        }
    }
}



