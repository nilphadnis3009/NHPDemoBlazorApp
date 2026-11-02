using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Shopping
{
    public class Prouduct_responseDTO
    {
        public Guid id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string seller { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool isInStock { get; set; }
        public int CurrentStock { get; set; }
    }
}
