using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class Supplier
    {
        public Supplier()
        {
            CategorySupplier1Navigation = new HashSet<Category>();
            CategorySupplier2Navigation = new HashSet<Category>();
            CategorySupplier3Navigation = new HashSet<Category>();
            PurchaseOrder = new HashSet<PurchaseOrder>();
        }

        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierEmail { get; set; }

        public ICollection<Category> CategorySupplier1Navigation { get; set; }
        public ICollection<Category> CategorySupplier2Navigation { get; set; }
        public ICollection<Category> CategorySupplier3Navigation { get; set; }
        public ICollection<PurchaseOrder> PurchaseOrder { get; set; }
    }
}
