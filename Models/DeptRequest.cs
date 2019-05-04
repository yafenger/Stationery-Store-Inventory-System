using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADProjectBase2.Models
{
    public partial class DeptRequest
    {
        public int DeptReqId { get; set; }
        public int DeptId { get; set; }
        public string ItemId { get; set; }
        public int? TotalQty { get; set; }

        [RegularExpression(@"[0-9]+", ErrorMessage = "Please enter a positive Integer!")]
        [Required(ErrorMessage = "Quantity is required")]
        [Column(TypeName = "numeric")]
        public int? ReceivedQty { get; set; }
        public DateTime? GeneratedTime { get; set; }
        public bool? IsCompleted { get; set; }

        public Department Dept { get; set; }
        public Item Item { get; set; }
    }
}
