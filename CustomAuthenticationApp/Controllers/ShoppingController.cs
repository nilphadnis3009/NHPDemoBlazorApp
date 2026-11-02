using CustomAuthenticationApp.Context;
using Entities.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomAuthenticationApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShoppingController(AppDbContext context)
        {
            _context = context;
        }

        //Following will get all products, and will have a flag of isInStock=1 || 0
        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<Prouduct_responseDTO>>> getAll()
        {
           List<Prouduct_responseDTO> result = await _context.Products.Select(p => new Prouduct_responseDTO
           {
               id = p.id,
               Name = p.Name,
               price = p.price,
               seller=p.Supplier,
               ImageUrl="",
               isInStock= (p.Stock == null || p.Stock.quantity <1)?false:true,
               CurrentStock = p.Stock.quantity==null ? 0 : p.Stock.quantity
               // Map any other properties from your entity to your DTO here
           }).ToListAsync();

            return Ok(result);
        }
    }
}
