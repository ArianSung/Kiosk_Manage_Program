using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin_Kiosk_Program
{
    public class PaymentMethod
    {
        public int PaymentId { get; set; }
        public string PaymentName { get; set; }
        public string PaymentImageUrl { get; set; }
        public Image PaymentImage { get; set; }
    }
}
