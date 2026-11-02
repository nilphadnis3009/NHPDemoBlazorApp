using CustomAuthenticationApp.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CustomAuthenticationApp.DynamicEngineWireframe
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider; // Use IServiceProvider to safely resolve Scoped service in Singleton wrapper if needed
        public PermissionAuthorizationHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true) return;

            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return;

            using var scope = _serviceProvider.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            if (await permissionService.UserHasPermissionAsync(userId, requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
