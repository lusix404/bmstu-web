using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using Microsoft.Extensions.Logging;
using CoffeeShops.ViewModels;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.Company;
using CoffeeShops.Services.Services;
using System.Security.Claims;
using System.Collections.Generic;

namespace CoffeeShops.Controllers.GUI;

[Route("api/v2/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CompanyGuiController : BaseGuiController
{
    private readonly ICompanyService _companyService;
    private readonly IMenuService _menuService;
    private readonly ILogger<CompanyGuiController> _logger;

    public CompanyGuiController(ICompanyService companyService, ILogger<CompanyGuiController> logger, IMenuService menuService)
    {
        _companyService = companyService;
        _logger = logger;
        _menuService = menuService;
    }

    [HttpGet("allcompanies")]
    public async Task<IActionResult> GetAllCompanies()
    {
        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "User");
        }

        try
        {
            var model = new CompaniesListViewModel();
            Guid cur_user_id = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id)
    ? id
    : Guid.Empty;
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
            int page = 1;
            int limit = 100;

            var filters = new CompanyFilters();

            var result = await _companyService.GetAllCompaniesAsync(filters, page, limit, cur_id_role);
            // Если пользователь не аутентифицирован или не удалось получить роль
            model.Companies = result.Data;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCompanies");
            ModelState.AddModelError("", "Произошла ошибка при получении списка компаний");
            return View(new CompaniesListViewModel());
        }
    }


    [HttpGet("menu")]
    public async Task<IActionResult> GetMenuByCompany(Guid companyId)
    {
        var model = new MenuListViewModel();

        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "User");
        }

        try
        {
            Guid cur_user_id = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id)
    ? id
    : Guid.Empty;
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
            int page = 1;
            int limit = 100;

            var result = await _menuService.GetMenuByCompanyIdAsync(companyId, page, limit, cur_id_role);
            model.Menu = result.Data;
            model.Id_company = companyId;

            foreach (var menuItem in model.Menu)
            {
                var drink = await _menuService.GetDrinkByIdAsync(menuItem.Id_drink, cur_id_role);
                if (drink != null)
                {
                    model.Drinks.Add(drink);
                }
            }

            model.Id_role = cur_id_role;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCompanies");
            ModelState.AddModelError("", "Произошла ошибка при получении списка компаний");
            return View(new MenuListViewModel());
        }
    }

    [HttpGet("companiesbydrink")]
    public async Task<IActionResult> GetCompaniesByDrink(Guid drink_id)
    {
        var model = new CompaniesListViewModel();

        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "User");
        }

        try
        {
            Guid cur_user_id = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid id)
    ? id
    : Guid.Empty;
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
            int page = 1;
            int limit = 100;

            var result = await _menuService.GetCompaniesByDrinkIdAsync(drink_id, page, limit, cur_id_role);
            model.Companies = result.Data;
            model.Id_role = cur_id_role;

            return View(model);
        }
        catch (CompaniesByDrinkNotFoundException)
        {
            model.Companies = new List<Company>();
            int cur_id_role = UserRoleExtensions.ToRoleIntFromString(User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
            model.Id_role = cur_id_role;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCompanies");
            ModelState.AddModelError("", "Произошла ошибка при получении списка компаний");
            return View(new MenuListViewModel());
        }
    }
}


