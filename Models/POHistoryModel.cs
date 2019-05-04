using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADProjectBase2.Models
{
    public class POHistoryModel
    {
        public List<PurchaseOrder> POList;
        public List<PODetails> podList;
        public List<double> pricelist;
        public SelectList startYear;
        public SelectList endYear;
        public SelectList startMonth;
        public SelectList endMonth;
        public int sy { get; set; }
        public int ey { get; set; }
        public string sm { get; set; }
        public string em { get; set; }
    }
}
