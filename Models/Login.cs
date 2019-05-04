using System;
using System.Collections.Generic;

namespace ADProjectBase2.Models
{
    public partial class Login
    {
        public Login()
        {
            MyUser = new HashSet<MyUser>();
        }

        public string Email { get; set; }
        public string Password { get; set; }

        public ICollection<MyUser> MyUser { get; set; }
    }
}
