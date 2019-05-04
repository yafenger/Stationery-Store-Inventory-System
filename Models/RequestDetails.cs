using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADProjectBase2.Models
{
    public partial class RequestDetails
    {
        public int ReqDetailsId { get; set; }
        public int RequestId { get; set; }
        public string ItemId { get; set; }
        [RegularExpression(@"[0-9]+", ErrorMessage = "Please enter a positive Integer!")]
        [Required(ErrorMessage = "Quantity is required")]
        [Column(TypeName = "numeric")]
        [Range(1, 1000, ErrorMessage = "Quantity should between 1 to 1000")]
        public int RequestedQty { get; set; }
        public string Type { get; set; }
        public bool? IsComplete { get; set; }

        public Item Item { get; set; }
        public Request Request { get; set; }
    }
}
