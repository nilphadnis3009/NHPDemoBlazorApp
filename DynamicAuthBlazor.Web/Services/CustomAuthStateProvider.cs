using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;

namespace DynamicAuthBlazor.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationState _anonymous;

        public CustomAuthStateProvider(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

                // Extract the claims array
                var claims = ParseClaimsFromJwt(token);

                // ✅ FIX: Explicitly tell Blazor to treat "name" (or "unique_name") as the Identity Name claim type
                // Use "jwt" as the authentication type so IsAuthenticated changes to true
                var identity = new ClaimsIdentity(claims, "jwt", "unique_name", "role");

                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return _anonymous;
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);

            // ✅ FIX: Mirror the exact same parameters here
            var identity = new ClaimsIdentity(claims, "jwt", "unique_name", "role"); 

            var authenticatedUser = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);

            // Parse the token into a generic JSON Element structure instead of a string dictionary
            using var jsonDocument = JsonDocument.Parse(jsonBytes);
            var root = jsonDocument.RootElement;

            foreach (var property in root.EnumerateObject())
            {
                // Check if this JWT key contains an array of items (like your list of permissions)
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in property.Value.EnumerateArray())
                    {
                        claims.Add(new Claim(property.Name, element.ToString()));
                    }
                }
                else
                {
                    // Handle standard singular text claims (like name, sub, exp)
                    claims.Add(new Claim(property.Name, property.Value.ToString()));
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
