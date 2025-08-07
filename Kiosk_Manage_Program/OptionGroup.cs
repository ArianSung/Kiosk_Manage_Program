using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin_Kiosk_Program
{
    public class OptionGroup
    {
        public int GroupId { get; set; }
        public int ProductId { get; set; }
        public string GroupName { get; set; }
        public bool IsRequired { get; set; }
        public bool AllowMultiple { get; set; }
        public List<Option> Options { get; set; } = new List<Option>();

        public OptionGroup()
        {
            Options = new List<Option>();
        }
    }
}
