using Microsoft.AspNetCore.Authorization;

namespace CustomAuthenticationApp.DynamicEngineWireframe
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission) => Permission = permission;
    }
}
