using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class CollectionPoint
    {
        public CollectionPoint()
        {
            Department = new HashSet<Department>();
        }

        public int Cpid { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string Location { get; set; }
        public string ImgUrl { get; set; }

        public ICollection<Department> Department { get; set; }
    }
}
