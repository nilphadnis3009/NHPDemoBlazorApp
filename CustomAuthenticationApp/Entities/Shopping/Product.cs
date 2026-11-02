using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Shopping
{
    public class Product
    {
        public Guid id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public string ImageUrl { get; set; }=string.Empty;

        public Stock? Stock { get; set; }
    }
}
