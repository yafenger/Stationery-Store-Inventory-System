using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ADProjectBase2.Models
{
    public class newAVModel
    {

        [Display(Name = "Adj. Voucher No.")]
        public int AdjustId { get; set; }

        public string UserName { get; set; }
        public string Status { get; set; }
        public int Amount { get; set; }
    }
}
