using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ADProjectBase2.Models
{
    public class RaiseOrderViewModel
    {
        public List<Item> items { get; set; }
        public List<PODetails> pods { get; set; }
        public List<SelectList> supplierGroups { get; set; }
        public List<Supplier> suppliers { get; set; }
        public SelectList supplierList;
        public string Suplist { get; set; }
        public List<int> preorderNums { get; set; }

        public IEnumerable<int> orderNums { get; set; }
        public ICollection<PODetails> podetails { get; set; }

        public string SearchString { get; set; }
        public string submitType { get; set; }
        public string Supplier { get; set; }
    }
}
