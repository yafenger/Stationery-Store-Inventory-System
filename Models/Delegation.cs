using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADProjectBase2.Models
{
    public class DateGreaterThan : ValidationAttribute
    {
        private string _startDatePropertyName;
        public DateGreaterThan(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);
            if ((DateTime)value > (DateTime)propertyValue)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("End date should be later than start date");
            }
        }
    }
    public partial class Delegation
    {
        public int DelegationId { get; set; }
        public int DeptId { get; set; }
        //*yafeng
        [Required(ErrorMessage = "Please select an employee!")]
        public int UserId { get; set; }
        [Column(TypeName = "datetime")]
        [Required(ErrorMessage = "This field is required.")]
        //*yafeng
        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTime? Startdate { get; set; }
        [Column(TypeName = "datetime")]
        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        [DateGreaterThan("Startdate")]
        public DateTime? Enddate { get; set; }

        public Department Dept { get; set; }
        public MyUser User { get; set; }
    }
}
