
using CustomAuthenticationApp.Context;
using CustomAuthenticationApp.DynamicEngineWireframe;
using CustomAuthenticationApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Cors.Infrastructure;

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 1. Define a name for your CORS policy
var blazorCorsPolicy = "_blazorCorsPolicy";

// 2. Add CORS services to the dependency injection container
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: blazorCorsPolicy,
        policy =>
        {
            policy.WithOrigins("https://localhost:7224", "http://localhost:5125", "https://icy-hill-0a58e4c10.4.azurestaticapps.net") // ⬅️ Replace with your exact Blazor App URLs
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required if you pass cookies or HttpContext identities
        });
});

// DB Layer & App Caches Setup
var connectionString = builder.Configuration.GetConnectionString("MysqlConnection");

// Register DbContext for MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString) // Automatically detects MySQL/MariaDB version
    ));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPermissionService, PermissionCacheService>();

// Identity Authentication Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

// Overriding Standard Policy Routing with Custom Providers
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddControllers();

// 2. REGISTER THE CORE AUTHENTICATION ENGINE
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT Key not configured");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSection.GetValue<string>("Issuer")),
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),

        ValidateAudience = !string.IsNullOrWhiteSpace(jwtSection.GetValue<string>("Audience")),
        ValidAudience = jwtSection.GetValue<string>("Audience"),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(60)
    };
});

var app = builder.Build();

// 2. Map the health check endpoint route
app.MapHealthChecks("/health");

app.UseHttpsRedirection();
app.UseCors(blazorCorsPolicy);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();