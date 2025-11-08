using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using CoffeeShops.Domain.Exceptions.MenuServiceExceptions;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using CoffeeShops.DTOs.Pagination;
using CoffeeShops.DTOs.User;

namespace CoffeeShops.Services.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IDrinkRepository _drinkRepository;
        private readonly ILogger<MenuService> _logger;

        public MenuService(IMenuRepository menuRepository, ICompanyRepository companyRepository, IDrinkRepository drinkRepository, ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _companyRepository = companyRepository;
            _drinkRepository = drinkRepository;
            _logger = logger;
        }

        public async Task<PaginatedResponse<Menu>>? GetMenuByCompanyIdAsync(Guid company_id, int page, int limit, int id_role)
        {
            _logger.LogInformation($"Attempting to retrieve menu for company with id={company_id}.");

            var company = await _companyRepository.GetCompanyByIdAsync(company_id, id_role);
            if (company == null)
            {
                _logger.LogWarning($"Company with id={company_id} was not found.");
                throw new CompanyNotFoundException($"Company with id={company_id} was not found");
            }

            (var menu, int total) = await _menuRepository.GetMenuByCompanyId(company_id, page, limit, id_role);
            if (menu == null || !menu.Any())
            {
                _logger.LogWarning($"Menu for company with id={company_id} was not found.");
                throw new MenuNotFoundException($"Menu for company with id={company_id} was not found");
            }

            _logger.LogInformation($"Menu for company with id={company_id} retrieved successfully.");
            return new PaginatedResponse<Menu> { Data = menu, Total = total, Limit = limit, Page = page };
        }

        public async Task<Drink?> GetDrinkByIdAsync(Guid drink_id, int id_role)
        {
            _logger.LogInformation($"Attempting to retrieve drink with id={drink_id}.");

            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning($"Drink with id={drink_id} was not found.");
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            _logger.LogInformation($"Drink with id={drink_id} retrieved successfully.");
            return drink;
        }

        public async Task<PaginatedResponse<Company>>? GetCompaniesByDrinkIdAsync(Guid drink_id, int page, int limit, int id_role)
        {
            _logger.LogInformation($"Attempting to retrieve companies for drink with id={drink_id}.");

            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning($"Drink with id={drink_id} was not found.");
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            (var companies, int total) = await _menuRepository.GetCompaniesByDrinkIdAsync(drink_id, page, limit,id_role);
            if (companies == null || !companies.Any())
            {
                _logger.LogWarning($"There are no companies with drink(id={drink_id}) in their menu.");
                throw new CompaniesByDrinkNotFoundException($"There is no companies with drink(id={drink_id}) in their menu");
            }

            _logger.LogInformation($"Retrieved {companies.Count} companies for drink with id={drink_id}.");
            return new PaginatedResponse<Company> { Data = companies, Total = total, Limit = limit, Page = page };
        }

        public async Task AddDrinkToMenuAsync(Menu menurecord, int id_role)
        {
            if (menurecord == null)
            {
                _logger.LogWarning("Attempted to add a null menu record.");
                throw new ArgumentNullException(nameof(menurecord));
            }

            if (menurecord.Size == 0)
            {
                _logger.LogWarning("Drink's size in menu name cannot be empty.");
                throw new MenuIncorrectAtributeException("Drink's size in menu name cannot be empty");
            }

            if (menurecord.Price <= 0)
            {
                _logger.LogWarning("Drink's price in menu must be > 0.");
                throw new MenuIncorrectAtributeException("Drink's price in menu must be > 0");
            }

            await _menuRepository.AddAsync(menurecord, id_role);
            _logger.LogInformation($"Drink with id={menurecord.Id_drink} added to the menu successfully.");
        }

        //public async Task DeleteDrinkFromMenuAsync(Guid drink_id, Guid company_id, int id_role)
        //{
        //    _logger.LogInformation($"Attempting to delete drink with id={drink_id} from menu of company with id={company_id}.");

        //    var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
        //    if (drink == null)
        //    {
        //        _logger.LogWarning($"Drink with id={drink_id} was not found.");
        //        throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
        //    }

        //    var company = await _companyRepository.GetCompanyByIdAsync(company_id, id_role);
        //    if (company == null)
        //    {
        //        _logger.LogWarning($"Company with id={company_id} was not found.");
        //        throw new CompanyNotFoundException($"Company with id={company_id} was not found");
        //    }

        //    await _menuRepository.RemoveAsync(drink_id, company_id, id_role);
        //    _logger.LogInformation($"Drink with id={drink_id} has been deleted from the menu of company with id={company_id}.");
        //}
        public async Task DeleteDrinkFromMenuAsync(Guid drink_id, Guid company_id, int id_role)
        {
            _logger.LogInformation($"Attempting to delete drink with id={drink_id} from menu of company with id={company_id}.");

            var drink = await _drinkRepository.GetDrinkByIdAsync(drink_id, id_role);
            if (drink == null)
            {
                _logger.LogWarning($"Drink with id={drink_id} was not found.");
                throw new DrinkNotFoundException($"Drink with id={drink_id} was not found");
            }

            var company = await _companyRepository.GetCompanyByIdAsync(company_id, id_role);
            if (company == null)
            {
                _logger.LogWarning($"Company with id={company_id} was not found.");
                throw new CompanyNotFoundException($"Company with id={company_id} was not found");
            }

            await _menuRepository.RemoveAsync(drink_id, company_id, id_role);
            _logger.LogInformation($"Drink with id={drink_id} has been deleted from the menu of company with id={company_id}.");
        }

    }
}

