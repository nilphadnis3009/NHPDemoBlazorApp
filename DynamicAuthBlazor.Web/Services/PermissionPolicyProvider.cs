using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DynamicAuthBlazor.Web.Services
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Check if the policy string is intended for permissions (e.g., matching Approach 1 or 2 prefixes)
            if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
            {
                var permissionName = policyName.Substring("Permission:".Length);

                var policy = new AuthorizationPolicyBuilder();
                // Demands that the authenticated user principal holds the matching claim type array value
                policy.RequireClaim("permission", permissionName);
                return policy.Build();
            }

            // Fallback to standard policy mapping (for Roles or basic authentication markers)
            return await base.GetPolicyAsync(policyName);
        }
    }
}
