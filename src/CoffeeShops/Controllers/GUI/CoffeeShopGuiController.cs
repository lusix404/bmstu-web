using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.CoffeeShopServiceExceptions;
using CoffeeShops.Domain.Exceptions.CompanyServiceExceptions;
using Microsoft.Extensions.Logging;
using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;
using CoffeeShops.Domain.Exceptions.UserServiceExceptions;
using CoffeeShops.Services.Services;
using CoffeeShops.ViewModels;
using System.Security.Claims;
using CoffeeShops.Domain.Models.Enums;
using CoffeeShops.DTOs.CoffeeShop;
using CoffeeShops.DTOs.Company;


namespace CoffeeShops.Controllers.GUI;

[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CoffeeShopGuiController : BaseGuiController
{

    private readonly ICoffeeShopService _CoffeeShopService;
    private readonly ICompanyService _CompanyService;
    private readonly ILogger<CoffeeShopGuiController> _logger;
    private readonly IFavCoffeeShopsService _favcoffeeshopsService;
    public CoffeeShopGuiController(ICoffeeShopService CoffeeShopService, IFavCoffeeShopsService favcoffeeshopsService, ILogger<CoffeeShopGuiController> logger, ICompanyService companyService)
    {
        _CoffeeShopService = CoffeeShopService;
        _favcoffeeshopsService = favcoffeeshopsService;
        _logger = logger;
        _CompanyService = companyService;
    }

    [HttpGet("coffeeshops")]
    public async Task<IActionResult> GetCoffeeShopsByCompany(Guid companyId)
    {

        //var model = new CoffeeShopsListViewModel();
        //try
        //{
        //    var user = HttpContext.User;
        //    var roleString = user.FindFirst("Id_role");
        //    if (user.Identity is not null && user.Identity.IsAuthenticated)
        //    {
        //        try
        //        {
        //            int id_role = int.Parse(roleString.Value);
        //            model.CoffeeShops = await _CoffeeShopService.GetCoffeeShopsByCompanyIdAsync(companyId, id_role);
        //            model.Id_role = id_role;
        //            model.Id_company = companyId;
        //            return View(model);
        //        }
        //        catch (InvalidCastException e)
        //        {
        //            ModelState.AddModelError("", "Произошла ошибка во время получении данных о кофейне");
        //            _logger.LogError(e, e.Message);
        //            return View();
        //        }
        //    }

        //    try
        //    {
        //        model.CoffeeShops = await _CoffeeShopService.GetCoffeeShopsByCompanyIdAsync(companyId, 1);

        //        return View(model);
        //    }
        //    catch (Exception e)
        //    {
        //        ModelState.AddModelError("", "Произошла ошибка во время получении данных о кофейне");
        //        _logger.LogError(e, e.Message);

        //        return View();
        //    }
        //}
        //catch (Exception e)
        //{
        //    ModelState.AddModelError("", "Произошла ошибка во время получении данных о кофейне");
        //    _logger.LogError(e, e.Message);
        //    return View();
        //}
        var model = new CoffeeShopsListViewModel();

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
            var filters = new CoffeeShopFilters();
            var result = await _CoffeeShopService.GetCoffeeShopsByCompanyIdAsync(companyId, filters, page, limit, cur_id_role);
            model.CoffeeShops = result.Data;
            model.Id_company = companyId;
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

    //[HttpGet]
    //public IActionResult AddCoffeeShopByCompany(Guid id_company)
    //{
    //    AddCoffeeShopViewModel model = new AddCoffeeShopViewModel();
    //    model.Id_company = id_company;
    //    var user = HttpContext.User;
    //    var roleString = user.FindFirst("Id_role");
    //    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
    //    model.Id_role = id_role;
    //    return View(model);
    //}



    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> AddCoffeeShopByCompany(AddCoffeeShopViewModel model)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        try
    //        {
    //            var user = HttpContext.User;
    //            var roleString = user.FindFirst("Id_role");


    //            if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
    //            {

    //                int id_role = int.Parse(roleString.Value);
    //                var cs = new CoffeeShop(model.Id_company, model.Address, model.WorkingHours);
    //                await _CoffeeShopService.AddCoffeeShopAsync(cs, id_role);
    //                //return RedirectToAction("GetCoffeeShopsByCompany", "CoffeeShop");
    //                return RedirectToAction("GetCoffeeShopsByCompany", new
    //                {
    //                    companyId = model.Id_company, // или другое значение
    //                    message = "Coffee shop added successfully"
    //                });
    //            }
    //            else
    //            {
    //                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //                _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //            }
    //        }
    //        catch (CoffeeShopIncorrectAtributeException e)
    //        {
    //            ModelState.AddModelError("", "Данные введены некорректно");
    //            _logger.LogError(e, e.Message);
    //            return View(model);
    //        }
    //        catch (InvalidDataException e)
    //        {
    //            ModelState.AddModelError("", "Данные введены некорректно");
    //            _logger.LogError(e, e.Message);
    //            return View(model);
    //        }
    //        catch (Exception e)
    //        {
    //            ModelState.AddModelError("", "Возникла ошибка при добавлении кофейни");
    //            _logger.LogError(e, e.Message);
    //            return View(model);
    //        }
    //    }

    //    else
    //    {

    //        _logger.LogWarning("ModelState is invalid");
    //        foreach (var error in ModelState)
    //        {
    //            _logger.LogWarning($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
    //        }
    //        return View(model);
    //    }
    //    return RedirectToAction("GetAllCompanies", "Company");
    //}


    //public async Task<IActionResult> DeleteCoffeeShop(Guid id_cs, Guid id_comp)
    //{
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var roleString = user.FindFirst("Id_role");


    //        if (user.Identity is not null && user.Identity.IsAuthenticated && roleString is not null)
    //        {
    //            int id_role = int.Parse(roleString.Value);
    //            await _CoffeeShopService.DeleteCoffeeShopAsync(id_cs, id_role);
    //            return RedirectToAction("GetCoffeeShopsByCompany", new
    //            {
    //                companyId = id_comp,
    //                message = "Coffee shop deleted successfully"
    //            });
    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }

    //    }
    //    catch (InvalidDataException e)
    //    {
    //        _logger.LogInformation(e.Message);
    //        ModelState.AddModelError("", "Произошла ошибка при удалении кофейни");
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogInformation(e.Message);
    //        ModelState.AddModelError("", "Произошла ошибка при удалении кофейни");
    //    }
    //    return RedirectToAction("GetCoffeeShopsByCompany", new
    //    {
    //        companyId = id_comp,
    //        message = "Coffee shop deleted successfully"
    //    });
    //}


    //public async Task<IActionResult> DeleteFromFavs(Guid coffeeshop_id)
    //{
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            // var id_user = Int32.Parse(id.Value);
    //            var id_user = Guid.Parse(id.Value);
    //            int id_role = int.Parse(roleString.Value);
    //            await _favcoffeeshopsService.RemoveCoffeeShopFromFavsAsync(id_user, coffeeshop_id, id_role);
    //            return RedirectToAction("GetFavCoffeeShops", new
    //            {
    //                message = "coffeeshop removed from favorites successfully"
    //            });
    //        }


    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (UserNotFoundException e)
    //    {
    //        _logger.LogError(e, $"User not found while removing favorite coffeeshop {coffeeshop_id}");
    //        ModelState.AddModelError("", "User not found");
    //    }
    //    catch (CoffeeShopNotFoundException e)
    //    {
    //        _logger.LogError(e, $"coffeeshop {coffeeshop_id} not found while removing from favorites");
    //        ModelState.AddModelError("", "coffeeshop not found");
    //    }
    //    catch (CoffeeShopIsNotFavoriteException e)
    //    {
    //        _logger.LogError(e, $"coffeeshop {coffeeshop_id} was not in favorites");
    //        ModelState.AddModelError("", "coffeeshop was not in favorites");
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError(e, $"Error removing coffeeshop {coffeeshop_id} from favorites");
    //        ModelState.AddModelError("", "Error removing from favorites");
    //    }

    //    return RedirectToAction("GetFavcoffeeshops");
    //}

    //public async Task<IActionResult> AddToFavs(Guid coffeeshop_id, Guid page)
    //{
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var id = User.FindFirst("Id");
    //        var roleString = user.FindFirst("Id_role");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            //var id_user = Int32.Parse(id.Value);
    //            var id_user = Guid.Parse(id.Value);
    //            int id_role = int.Parse(roleString.Value);
    //            await _favcoffeeshopsService.AddCoffeeShopToFavsAsync(id_user, coffeeshop_id, id_role);
    //            //if (page == -1)
    //            //{
    //            //    return RedirectToAction("GetAllCoffeeshops");
    //            //}
    //            return RedirectToAction("GetCoffeeShopsByCompany", new
    //            {
    //                companyId = page,
    //                message = ""
    //            });


    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (InvalidDataException e)
    //    {
    //        _logger.LogInformation(e.Message);
    //        ModelState.AddModelError("", "При добавлении в список избранных кофеен произошла ошибка");
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogInformation(e.Message);
    //        ModelState.AddModelError("", "При добавлении в список избранных кофеен произошла ошибка");
    //    }
    //    return RedirectToAction("GetFavcoffeeshops");
    //}

    //public async Task<IActionResult> GetFavCoffeeShops()
    //{
    //    var model = new FavCoffeeShopsListViewModel();
    //    try
    //    {
    //        var user = HttpContext.User;
    //        var roleString = user.FindFirst("Id_role");
    //        var id = User.FindFirst("Id");
    //        if (user.Identity is not null && user.Identity.IsAuthenticated)
    //        {
    //            //var id_user = Int32.Parse(id.Value);
    //            var id_user = Guid.Parse(id.Value);
    //            int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
    //            var pairs = await _favcoffeeshopsService.GetFavCoffeeShopsAsync(id_user, id_role);

    //            foreach (var c in pairs)
    //            {
    //                var cs = await _CoffeeShopService.GetCoffeeShopByIdAsync(c.Id_coffeeshop, id_role);
    //                var comp = await _CompanyService.GetCompanyByIdAsync(cs.Id_company, id_role);
    //                var cofcomp = new CoffeeShopWithCompany
    //                {
    //                    CompanyName = comp.Name,
    //                    CoffeeShop = cs
    //                };


    //                model.FavCoffeeShops.Add(cofcomp);
    //            }

    //            return View(model);



    //        }
    //        else
    //        {
    //            ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
    //            _logger.LogError("", "Пользователь не прошел проверку подлинности");
    //        }
    //    }
    //    catch (InvalidCastException e)
    //    {
    //        _logger.LogError(e, e.Message);
    //        ModelState.AddModelError("", "При получении списка избранных кофеен произошла ошибка");
    //        return View();
    //    }
    //    catch (Exception e)
    //    {
    //        ModelState.AddModelError("", "При получении списка избранных кофеен произошла ошибка");
    //        _logger.LogError(e, e.Message);
    //        return View();
    //    }

    //    return RedirectToAction("GetAllCompanies", "Company");
    //}
}



