using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
namespace DynamicAuthBlazor.Web.Services
{
    [Authorize]
    public class CartService
    {
        private readonly HttpClient _http;
        private List<CartItem> _items = new();

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
        public event Action? OnCartChanged;
        private readonly AuthenticationStateProvider _authStateProvider;
        public CartService(HttpClient http, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        private void NotifyStateChanged() => OnCartChanged?.Invoke();

        // Call this inside MainLayout.razor or Page initialization once user authenticates
        public async Task<List<CartItem>> LoadCartFromDatabaseAsync()
        {
            try
            {

                var serverItems = await _http.GetFromJsonAsync<List<CartItem>>("api/Cart/GetCart");
                if (serverItems != null)
                {
                    _items = serverItems;
                    OnCartChanged?.Invoke();
                }
               
                return serverItems;


            }
            catch(Exception ex) 
            {
                // Fail silently or handle unauthenticated users gracefully
                _items = new List<CartItem>();
            }
            return _items;
        }

        public async Task AddToCartAsync(CartItem item)
        {
            // 1. Optimistic UI update or API sync first
            var response = await _http.PostAsJsonAsync("api/cart/add", item);
            if (response.IsSuccessStatusCode)
            {
                var existingItem = _items.FirstOrDefault(i => i.product_uid == item.product_uid);
                if (existingItem != null)
                {
                    if (existingItem.Quantity < item.MaxStock) existingItem.Quantity++;
                }
                else
                {
                    _items.Add(item);
                }
                NotifyStateChanged();
            }
        }

        public async Task UpdateQuantityAsync(Guid productId, int newQuantity)
        {
            var response = await _http.PutAsync($"api/cart/update/{productId}/{newQuantity}", null);
            if (response.IsSuccessStatusCode)
            {
                var item = _items.FirstOrDefault(i => i.product_uid == productId);
                if (item != null)
                {
                    item.Quantity = newQuantity;
                    NotifyStateChanged();
                }
            }
        }

        public async Task RemoveFromCartAsync(Guid productId)
        {
            var response = await _http.DeleteAsync($"api/cart/remove/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var item = _items.FirstOrDefault(i => i.product_uid == productId);
                if (item != null)
                {
                    _items.Remove(item);
                    NotifyStateChanged();
                }
            }
        }

        public int GetTotalCount() => _items.Sum(i => i.Quantity);
        public decimal GetTotalPrice() => _items.Sum(i => i.Price * i.Quantity);
    }
}
   
