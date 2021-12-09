using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBurger.Products.Contract
{
    public class ProductChanged
    {
        public ProductState State { get; set; }
        public Product Product { get; set; }
    }

}
