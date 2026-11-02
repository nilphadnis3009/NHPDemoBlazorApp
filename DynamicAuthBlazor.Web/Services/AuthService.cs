using Entities.UIModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace DynamicAuthBlazor.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
        }

        public async Task<LoginResultDto> LoginAsync(LoginModel loginModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new LoginResultDto { IsSuccess = false, ErrorMessage = error };
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResultDto>();
                if (result != null && result.IsSuccess && !string.IsNullOrEmpty(result.Token))
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                    return new LoginResultDto { IsSuccess = true,Token= result.Token };
                }
                return new LoginResultDto { IsSuccess = false, ErrorMessage = "Invalid authentication response." };
            }
            catch (Exception ex)
            {
                return new LoginResultDto { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }
    }
}
