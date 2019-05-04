using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class PurchaseOrder
    {
        public PurchaseOrder()
        {
            Podetails = new HashSet<PODetails>();
        }

        public int PoId { get; set; }
        public int SupplierId { get; set; }
        public DateTime PurchaseDate { get; set; }

        public Supplier Supplier { get; set; }
        public ICollection<PODetails> Podetails { get; set; }
    }
}
