using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace ADProjectBase2.Models
{
    public class DeptRequestsViewModel
    {
        public List<DeptRequest> deptRequests;
        public List<GroupByItemView> byItemList;
        public List<string> binlist;
        public SelectList deptList;
        public string SubmitType { get; set; }
        public string SearchType { get; set; }
        public string SearchString { get; set; }

    }
}
