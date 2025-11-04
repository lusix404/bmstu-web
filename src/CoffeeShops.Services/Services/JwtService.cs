using CoffeeShops.Services.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffeeShops.Services.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string login, int idRole)
    {
        var claims = new[]
        {
           new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
           new Claim(ClaimTypes.Name, login),
           new Claim(ClaimTypes.Role, idRole.ToString()),
           //new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
           new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())  //JSON Token Identifier

        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);  //возвращает строку в формате JWT
    }
}
