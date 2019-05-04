using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class PODetails
    {
        public int PodetailsId { get; set; }
        public int PoId { get; set; }
        public string ItemId { get; set; }
        public int? QtyOrdered { get; set; }

        public Item Item { get; set; }
        public PurchaseOrder Po { get; set; }
    }
}
