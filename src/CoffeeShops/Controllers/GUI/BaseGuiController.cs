using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeShops.Controllers.GUI
{
    public class BaseGuiController : Controller
    {
        protected string GetJwtToken()
        {
            // Получаем токен из cookie
            return Request.Cookies["access_token"];
        }

        protected bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetJwtToken());
        }

        protected void AddJwtToViewData()
        {
            ViewData["JwtToken"] = GetJwtToken();
        }
    }
}