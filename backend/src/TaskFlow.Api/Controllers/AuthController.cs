using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized("Invalid email or password.");
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            return Unauthorized("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateToken(user, roles);

        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        });
    }

    private string GenerateToken(AppUser user, IEnumerable<string> roles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"] ?? string.Empty;
        var issuer = jwtSettings["Issuer"] ?? string.Empty;
        var audience = jwtSettings["Audience"] ?? string.Empty;
        var durationInMinutes = int.TryParse(jwtSettings["DurationInMinutes"], out var minutes) ? minutes : 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
