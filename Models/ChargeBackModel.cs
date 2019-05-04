using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADProjectBase2.Models
{
    public class ChargeBackModel
    {
        public List<DeptRequest> deptRequests;
        public List<double> amountList; 
        public SelectList deptList;
        public double tot { get; set; } 
        public string SearchType { get; set; }
        public string SearchString { get; set; }
        public string startTime { get; set; }
    }
}
