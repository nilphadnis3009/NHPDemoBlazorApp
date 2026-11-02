using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CustomAuthenticationApp.DynamicEngineWireframe
{
    public class DynamicPermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null) return policy;

            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }

}
