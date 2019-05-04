using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADProjectBase2.Models
{
    public partial class Item
    {
        public Item()
        {
            Avdetails = new HashSet<AVDetails>();
            DeptRequest = new HashSet<DeptRequest>();
            Podetails = new HashSet<PODetails>();
            RequestDetails = new HashSet<RequestDetails>();
        }
        [Required(ErrorMessage = "Field is required")]
        [RegularExpression(@"[A-Z]{1}\d{3}", ErrorMessage = "Please enter a valid ItemID. For e.g. C001")]
        public string ItemId { get; set; }
        [Required]
        [StringLength(60, MinimumLength = 3)]
        public string ItemName { get; set; }
        public int CatId { get; set; }
        [Required]
        [Column(TypeName = "numeric")]
        [Range(1, 1500)]
        public int? ReorderLvl { get; set; }
        [Required]
        [Column(TypeName = "numeric")]
        [Range(1, 2000)]
        public int? ReorderQty { get; set; }
        [Required]
        [Display(Name = "Unit of Measurement")]
        public string Uom { get; set; }
        [Required]
        [Column(TypeName = "numeric")]
        [Range(0, 2000)]
        public int? Stock { get; set; }
        [Required]
        [Range(0, 100)]
        [DataType(DataType.Currency)]
        public int? Unitprice { get; set; }

        public Category Cat { get; set; }
        public ICollection<AVDetails> Avdetails { get; set; }
        public ICollection<DeptRequest> DeptRequest { get; set; }
        public ICollection<PODetails> Podetails { get; set; }
        public ICollection<RequestDetails> RequestDetails { get; set; }
    }
}
