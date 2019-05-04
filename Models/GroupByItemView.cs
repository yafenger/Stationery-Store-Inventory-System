using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace ADProjectBase2.Models
{
    public class GroupByItemView
    {       
        public string BinNumber { get; set; }
        public string ItemName { get; set; }
        public int TotalQuantity { get; set; }
    }
}
