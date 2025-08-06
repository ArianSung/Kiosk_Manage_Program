using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin_Kiosk_Program
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImageUrl { get; set; }
        public Image ProductImage { get; set; }
        public List<OptionGroup> OptionGroups { get; set; }

        public Product()
        {
            OptionGroups = new List<OptionGroup>();
        }
    }
}
