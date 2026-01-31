//using Microsoft.AspNetCore.Mvc;
//using CoffeeShops.Domain.Models;
//using CoffeeShops.Services.Interfaces.Services;
//using Swashbuckle.AspNetCore.Annotations;
//using System.ComponentModel.DataAnnotations;
//using CoffeeShops.Domain.Exceptions.CategoryServiceExceptions;
//using CoffeeShops.Domain.Exceptions.DrinkServiceExceptions;

//namespace CoffeeShops.Controllers
//{
//    [ApiController]
//    [Route("api/drinks-categories")]
//    [Produces("application/json")]
//    public class DrinksCategoriesController : ControllerBase
//    {
//        private readonly IDrinksCategoryService _drinksCategoryService;

//        public DrinksCategoriesController(IDrinksCategoryService drinksCategoryService)
//        {
//            _drinksCategoryService = drinksCategoryService;
//        }

//        [HttpPost]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Assign category to drink",
//            Description = "Creates a relationship between a drink and a category",
//            OperationId = "AssignCategoryToDrink")]
//        public async Task<IActionResult> AssignCategory(
//            [FromQuery]
//            [SwaggerParameter(Description = "Unique identifier of the drink", Required = true)]
//            Guid drinkId,
//            [FromQuery]
//            [SwaggerParameter(Description = "Unique identifier of the category", Required = true)]
//            Guid categoryId)
//        {
//            try
//            {
//                await _drinksCategoryService.AddAsync(drinkId, categoryId);
//                return Ok();
//            }
//            catch (DrinkNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (CategoryNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (DrinkAlreadyHasThisCategoryException ex)
//            {
//                return Conflict(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }

       
//        [HttpDelete("{drinkId:guid}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Remove all categories from drink",
//            Description = "Clears all category associations for a specific drink",
//            OperationId = "RemoveAllCategoriesFromDrink")]
//        public async Task<IActionResult> RemoveAllCategories(
//            [FromRoute]
//            [SwaggerParameter(Description = "Unique identifier of the drink", Required = true)]
//            Guid drinkId)
//        {
//            try
//            {
//                await _drinksCategoryService.RemoveAsync(drinkId);
//                return Ok();
//            }
//            catch (DrinkNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }

       
//        [HttpGet("{drinkId:guid}/categories")]
//        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
//        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
//        [SwaggerOperation(
//            Summary = "Get drink categories",
//            Description = "Retrieves all categories associated with a specific drink",
//            OperationId = "GetDrinkCategories")]
//        public async Task<IActionResult> GetDrinkCategories(
//            [FromRoute]
//            [SwaggerParameter(Description = "Unique identifier of the drink", Required = true)]
//            Guid drinkId)
//        {
//            try
//            {
//                var categories = await _drinksCategoryService.GetCategoryByDrinkIdAsync(drinkId);
//                return Ok(categories);
//            }
//            catch (DrinkNotFoundException ex)
//            {
//                return NotFound(ex.Message);
//            }
//            catch (Exception)
//            {
//                return StatusCode(500, "Internal server error");
//            }
//        }
//    }
//}