using Microsoft.AspNetCore.Mvc;

namespace CoffeeShops.DTOs.Utils;

public static class ErrorMapper
{
    public static ActionResult MapErrorToActionResult(Error error)
    {
        return error.Code switch
        {
            400 => new BadRequestObjectResult(error),
            401 => new UnauthorizedObjectResult(error),
            404 => new NotFoundObjectResult(error),
            _ => new BadRequestObjectResult(error)
        };
    }

    public static IActionResult ToActionResult(this Error error)
    {
        return error.Code switch
        {
            400 => new BadRequestObjectResult(error),
            401 => new UnauthorizedObjectResult(error),
            404 => new NotFoundObjectResult(error),
            _ => new BadRequestObjectResult(error)
        };
    }
}