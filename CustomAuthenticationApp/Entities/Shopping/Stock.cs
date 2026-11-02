using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Shopping
{
    public class Stock
    {
        public int Id { get; set; }
        public Guid product_uid { get; set; }
        public int quantity { get; set; } = 0;
        public DateTime date_added { get; set; }
        public DateTime date_modified { get; set; }
        public int modified_by { get; set; }

        public Product? product { get; set; } = null!;
    }
}
