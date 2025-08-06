using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin_Kiosk_Program
{
    public class OrderItem
    {
        public Product BaseProduct { get; }
        public Dictionary<int, Option> SelectedOptions { get; }
        public int Quantity { get; set; } = 1;
        public decimal TotalPrice => (BaseProduct.BasePrice + SelectedOptions.Values.Sum(opt => opt.AdditionalPrice)) * Quantity;

        public OrderItem(Product baseProduct, Dictionary<int, Option> selectedOptions)
        {
            BaseProduct = baseProduct;
            SelectedOptions = new Dictionary<int, Option>(selectedOptions);
        }
    }
}
