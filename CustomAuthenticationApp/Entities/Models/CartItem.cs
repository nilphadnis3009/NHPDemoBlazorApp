using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class CartItem
    {
        public int cid { get; set; }
        public Guid product_uid { get; set; }
        public int user_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MaxStock { get; set; } // Prevents adding more than available stock
    }
}
