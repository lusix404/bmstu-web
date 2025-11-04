using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.LoyaltyProgramServiceExceptions;
using CoffeeShops.ViewModels;

namespace CoffeeShops.Controllers;
public class LoyaltyProgramController : Controller
{
    private readonly ILoyaltyProgramService _loyaltyProgramService;
    private readonly ILogger<LoyaltyProgramController> _logger;
    public LoyaltyProgramController(ILoyaltyProgramService loyaltyProgramService, ILogger<LoyaltyProgramController> logger)
    {
        _loyaltyProgramService = loyaltyProgramService;
        _logger = logger;
    }

    public async Task<IActionResult> GetLpById(Guid lp_id)
    {
        try
        {
            var lp = new LoyaltyProgramViewModel();
            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                int id_role = int.Parse(roleString.Value);
                lp.LoyaltyProgram = await _loyaltyProgramService.GetLoyaltyProgramByIdAsync(lp_id, id_role);
                return View(lp);


            }
            else
            {
                ModelState.AddModelError("", "Пользователь не прошел проверку подлинности");
                _logger.LogError("", "Пользователь не прошел проверку подлинности");
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message);
            ModelState.AddModelError("", "Произошла ошибка при получении сведений о программе лояльности");
            return View();
        }

        return RedirectToAction("Login", "User");
    }
}