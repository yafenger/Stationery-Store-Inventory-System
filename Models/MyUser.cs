using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class MyUser
    {
        public MyUser()
        {
            AdjustmentVoucher = new HashSet<AdjustmentVoucher>();
            Delegation = new HashSet<Delegation>();
            Request = new HashSet<Request>();
        }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public int DeptId { get; set; }

        public Department Dept { get; set; }
        public Login EmailNavigation { get; set; }
        public Role Role { get; set; }
        public ICollection<AdjustmentVoucher> AdjustmentVoucher { get; set; }
        public ICollection<Delegation> Delegation { get; set; }
        public ICollection<Request> Request { get; set; }
    }
}
