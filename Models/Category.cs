using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class Category
    {
        public Category()
        {
            Item = new HashSet<Item>();
        }

        public int CatId { get; set; }
        public string CatName { get; set; }
        public int Supplier1 { get; set; }
        public int Supplier2 { get; set; }
        public int Supplier3 { get; set; }

        public Supplier Supplier1Navigation { get; set; }
        public Supplier Supplier2Navigation { get; set; }
        public Supplier Supplier3Navigation { get; set; }
        public ICollection<Item> Item { get; set; }
    }
}
