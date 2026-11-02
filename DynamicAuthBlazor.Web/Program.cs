using DynamicAuthBlazor.Web;
using DynamicAuthBlazor.Web.Providers;
using DynamicAuthBlazor.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

/// Register base authorization infrastructure
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<NotificationStateService>();
builder.Services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>());
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddAuthorizationCore(options =>
{
    // 🛡️ Hardcode the specific permission policy keys to completely satisfy the Blazor router
    options.AddPolicy("User.Create", policy => policy.RequireClaim("permission", "User.Create"));

    // Add any other core page keys you plan to use:
    options.AddPolicy("User.RunHeavyJob", policy => policy.RequireClaim("permission", "User.RunHeavyJob"));
    options.AddPolicy("User.View", policy => policy.RequireClaim("permission", "Student.Exam"));
    options.AddPolicy("Customer.All", policy => policy.RequireClaim("permission", "Customer.All"));
});
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7120") // ⬅️ Replace this with your exact Backend API localhost URL
});

// Blazor Authentication State Pipeline Integration
builder.Services.AddAuthorizationCore();


await builder.Build().RunAsync();
