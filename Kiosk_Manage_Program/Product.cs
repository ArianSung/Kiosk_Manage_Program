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
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImageUrl { get; set; } // VARCHAR(URL)을 저장할 속성
        public Image ProductImage { get; set; }      // URL을 통해 로드된 이미지를 임시 저장
        public int ProductKcal { get; set; }
        public List<OptionGroup> OptionGroups { get; set; } = new List<OptionGroup>();

        public Product()
        {
            OptionGroups = new List<OptionGroup>();
        }
    }
}
