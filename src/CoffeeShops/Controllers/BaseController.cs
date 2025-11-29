using CoffeeShops.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeShops.Controllers;

public class BaseController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found");
    }

    protected string GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(roleClaim))
        {
            throw new UnauthorizedAccessException("User role not found");
        }
        return roleClaim;
    }

    protected string GetCurrentUserLogin()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }
}
