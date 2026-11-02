using Entities.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace DynamicAuthBlazor.Web.Services
{
    [Authorize]
    public class ProductRepository : IProductRepository
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
       

        public ProductRepository(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<IEnumerable<Prouduct_responseDTO>> getAllProductsAsync()
        {
            var resp = await _httpClient.GetFromJsonAsync<List<Prouduct_responseDTO>>("api/Shopping/getAll");
            return resp ?? Enumerable.Empty<Prouduct_responseDTO>();
        }

       
    }
}
