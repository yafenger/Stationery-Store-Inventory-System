using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ADProjectBase2.Models
{
    public partial class Request
    {
        public Request()
        {
            RequestDetails = new HashSet<RequestDetails>();
        }

        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime? Approvaltime { get; set; }
        public string Remarks { get; set; }
        public bool? IsCompleted { get; set; }
        [DisplayName("Employee")]
        public MyUser User { get; set; }
        public ICollection<RequestDetails> RequestDetails { get; set; }
    }
}
