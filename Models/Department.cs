using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ADProjectBase2.Models
{
    public partial class Department
    {
        public Department()
        {
            Delegation = new HashSet<Delegation>();
            DeptRequest = new HashSet<DeptRequest>();
            MyUser = new HashSet<MyUser>();
        }

        public int DeptId { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Please select an employee")]
        public int? DeptRep { get; set; }
        [Required(ErrorMessage = "Please select a collection point!")]
        public int Cpid { get; set; }

        public CollectionPoint Cp { get; set; }
        public ICollection<Delegation> Delegation { get; set; }
        public ICollection<DeptRequest> DeptRequest { get; set; }
        public ICollection<MyUser> MyUser { get; set; }
    }
}
