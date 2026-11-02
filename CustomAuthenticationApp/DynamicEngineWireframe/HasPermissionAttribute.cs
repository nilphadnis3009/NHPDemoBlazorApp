using Microsoft.AspNetCore.Authorization;

namespace CustomAuthenticationApp.DynamicEngineWireframe
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(policy: permission) { }
    }
}
