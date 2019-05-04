using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADProjectBase2.Models
{
    public partial class AdjustmentVoucher
    {
        public AdjustmentVoucher()
        {
            Avdetails = new HashSet<AVDetails>();
        }

        [Display(Name = "Adj. Voucher No.")]
        public int AdjustId { get; set; }

        public int UserId { get; set; }
        public string Status { get; set; }

        public MyUser User { get; set; }
        public ICollection<AVDetails> Avdetails { get; set; }
    }
}
