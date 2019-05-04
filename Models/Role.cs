using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class Role
    {
        public Role()
        {
            MyUser = new HashSet<MyUser>();
        }

        public int RoleId { get; set; }
        public string Name { get; set; }

        public Role RoleNavigation { get; set; }
        public Role InverseRoleNavigation { get; set; }
        public ICollection<MyUser> MyUser { get; set; }
    }
}
