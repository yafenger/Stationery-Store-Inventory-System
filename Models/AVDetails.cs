using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ADProjectBase2.Models
{
    public partial class AVDetails
    {
        public int AVDid { get; set; }
        public int AdjustId { get; set; }
        public string ItemId { get; set; }

        [RegularExpression(@"[0-9]+", ErrorMessage = "Please enter a positive Integer!")]
        [Required(ErrorMessage = "Quantity/operation is required")]
        public int? Qtychanged { get; set; }
        [Required(ErrorMessage = "Quantity/operation is required")]
        public string Operations { get; set; }
        public string Remarks { get; set; }

        public AdjustmentVoucher Adjust { get; set; }
        public Item Item { get; set; }
    }
}
