using Microsoft.AspNetCore.Mvc;
using CoffeeShops.Domain.Models;
using CoffeeShops.Services.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
using CoffeeShops.ViewModels;

namespace CoffeeShops.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> GetAllCategories()
        {
            var model = new CategoriesListViewModel();

            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
                    model.Categories = await _categoryService.GetAllCategoriesAsync(id_role);
                    model.Id_role = id_role;
                    return View(model);
                }
                catch (InvalidCastException)
                {
                    ModelState.AddModelError("", "Data Access layer");
                    return View();
                }
            }

            try
            {
                model.Categories = await _categoryService.GetAllCategoriesAsync(1);

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Data Access layer");
                return View();
            }

        }

        public async Task<IActionResult> GetCategoriesByDrink()
        {
            var model = new CategoriesListViewModel();

            var user = HttpContext.User;
            var roleString = user.FindFirst("Id_role");
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                try
                {
                    int id_role = int.Parse(roleString.Value); // Преобразуем строку в int
                    model.Categories = await _categoryService.GetAllCategoriesAsync(id_role);
                    model.Id_role = id_role;
                    return View(model);
                }
                catch (InvalidCastException)
                {
                    ModelState.AddModelError("", "Data Access layer");
                    return View();
                }
            }

            try
            {
                model.Categories = await _categoryService.GetAllCategoriesAsync(1);

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Data Access layer");
                return View();
            }

        }

        //public async Task<IActionResult> AddCategory
        //public async Task<IActionResult> GetCategoriesByDrink()
    }
}