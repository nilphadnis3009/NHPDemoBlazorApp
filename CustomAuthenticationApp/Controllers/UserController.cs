using CustomAuthenticationApp.DynamicEngineWireframe;
using Entities.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthenticationApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [HasPermission("User.Read")]
        public IActionResult GetAllUsers() => Ok("Accessed sensitive data.");

        [HttpPost]
        [HasPermission("User.Create")]
        public IActionResult CreateUser() => Ok("User record provisioned.");

        [HttpGet("current")]
        public IActionResult GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

            return Ok(new UserProfileDto
            {
                Username = User.Identity.Name ?? string.Empty,
                Permissions = permissions
            });
        }
    }
}

    
