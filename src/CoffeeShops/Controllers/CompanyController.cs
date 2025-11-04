using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using Microsoft.Extensions.Logging;
using CoffeeShops.ViewModels;

namespace CoffeeShops.Controllers;
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    public async Task<IActionResult> GetAllCompanies()
    {
        try
        {
            var model = new CompaniesListViewModel();
            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                if (int.TryParse(roleString?.Value, out int id_role))
                {
                    model.Companies = await _companyService.GetAllCompaniesAsync(id_role);
                    model.Id_role = id_role;
                    return View(model);
                }
                else
                {
                    _logger.LogWarning("Failed to parse role ID");
                    ModelState.AddModelError("", "Ошибка определения роли пользователя");
                }
            }

            // Если пользователь не аутентифицирован или не удалось получить роль
            model.Companies = await _companyService.GetAllCompaniesAsync(1);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllCompanies");
            ModelState.AddModelError("", "Произошла ошибка при получении списка компаний");
            return View(new CompaniesListViewModel());
        }
    }
}