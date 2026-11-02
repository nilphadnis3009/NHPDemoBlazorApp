using CustomAuthenticationApp.Context;
using Entities.Models;
using Entities.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CustomAuthenticationApp.API.Controllers
{
    [Authorize] // Protect with your JWT Authentication
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Helper to extract the logged-in User ID from the JWT Token claims
        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        [HttpGet("GetCart")]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCart()
        {
            var userId = Convert.ToInt32(GetUserId());
            var items = await _context.CartItems.Where(c => c.user_id == userId).ToListAsync();
            return Ok(items);
        }
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(Guid productId)
        {
            var userId = Convert.ToInt32(GetUserId());
            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.user_id == userId && c.product_uid == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(CartItem itemDto)
        {
            var userId = Convert.ToInt32(GetUserId());

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.user_id == userId && c.product_uid == itemDto.product_uid);

            if (existingItem != null)
            {
                    var item = await _context.CartItems
                        .FirstOrDefaultAsync(c => c.user_id == userId && c.product_uid == itemDto.product_uid);

                    if (item != null && itemDto.Quantity > 0 && itemDto.Quantity <= item.MaxStock)
                    {
                        item.Quantity += itemDto.Quantity;
                        await _context.SaveChangesAsync();
                        return Ok();
                    }
                    return BadRequest("Invalid quantity or item missing.");
                


            }
            else
            {
                var cartItem = new CartItem
                {
                    product_uid = itemDto.product_uid,
                    MaxStock = itemDto.MaxStock,
                    ImageUrl = itemDto.ImageUrl,
                    Name = itemDto.Name,
                    Price = itemDto.Price,
                    Quantity = itemDto.Quantity,
                    user_id = userId

                };
                
                    _context.Add(cartItem);
                     _context.SaveChanges();
                    return Ok();
                
            }
                
        }
    }
}
