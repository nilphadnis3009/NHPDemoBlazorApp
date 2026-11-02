using CustomAuthenticationApp.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomAuthenticationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
       
        private readonly IConfiguration _configuration; // ⬅️ Add this private field

        // ⬇️ Inject IConfiguration through the constructor
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Username == request.Username);
            //string plainTextPassword = "Password123!";
            //string testHash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

            //bool doesVerifyWork = BCrypt.Net.BCrypt.Verify(plainTextPassword, testHash);


            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                // ❌ FIX: Return a structured JSON response instead of a plain text string
                return Unauthorized(new { IsSuccess = false, ErrorMessage = "Invalid credentials." });
            }
            string? roleName = user.UserRoles.Select(r => r.Role.Name).FirstOrDefault();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                 new Claim(ClaimTypes.Role, roleName)
            };

            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            // Generate a JWT Token to send back to the Blazor WASM client
            var tokenHandler = new JwtSecurityTokenHandler();

            // Ensure you have a strong key set up in your appsettings.json configuration
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretBackUpKeyThatIsLongEnough123!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // ✅ RETURN VALID JSON MATCHING YOUR BLAZOR 'LoginResultDto' 
            return Ok(new
            {
                IsSuccess = true,
                Token = tokenString
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logged out successfully.");
        }
    }

    public class LoginRequest
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
}
